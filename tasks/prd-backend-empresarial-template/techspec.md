# Especificação técnica

## Resumo

O template adota mini-monólito modular com quatro hosts independentes: API, Hangfire, polling e Event Hub. `Backend.App` define casos de uso e portas; `Backend.Domain` contém regras; `Backend.Contracts` contém eventos; projetos `Backend.Infra.*` implementam persistência, cache, jobs e integrações.

## Arquitetura do sistema

### Visão dos componentes

- `Backend.Api`: Controllers, Entra ID, Problem Details, OpenAPI e Scalar.
- `Backend.App`: commands, queries, handlers, validators e portas.
- `Backend.Domain`: Cadastro, Ticket, ChecklistItem e Integracao.
- `Backend.Contracts`: `TicketRecebidoV1` e `ResultadoChecklistEmpresaAV1`.
- `Backend.Infra.Postgres`: DbContext, mapeamentos e repositórios.
- `Backend.Infra.EventHub`: producer, processor e checkpoints Blob.
- `Backend.Infra.Hangfire`: cliente, servidor e ponte genérica para Cortex.
- `Backend.Infra.Gate.Sap`, `EmpresaA` e `EmpresaB`: adaptadores externos isolados.
- `Backend.Worker.*`: composição e handlers exclusivos de cada processo.

## Design de implementação

### Principais interfaces

```text
ICadastroRepository
  ListarAsync(tipo) -> cadastros
  SalvarAsync(cadastro) -> void

IBackgroundCommandScheduler
  Enqueue(command) -> jobId

IIntegracaoRepository
  BuscarEIniciarPendentesAsync(limite) -> integracoes reservadas
```

### Modelos de dados

#### `Cadastro` — cadastro mestre

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `id` | UUID | sim | Identificador local |
| `tipo` | enum | sim | Fazenda, AnoAgricola, Safra ou Cultura |
| `codigo` | string | sim | Código funcional |
| `nome` | string | sim | Nome exibido |
| `codigoExterno` | string | não | Chave no SAP |

```text
{"id":"51c91fec-6602-4821-9af1-73031e6acc5c","tipo":"Fazenda","codigo":"F001","nome":"Fazenda Sul","codigoExterno":"SAP-10"}
```

#### `TicketRecebidoV1` — evento de entrada

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `eventId` | string | sim | Chave de idempotência |
| `numeroTicket` | string | sim | Identificador externo |
| `correlationId` | string | sim | Correlação distribuída |
| `recebidoEm` | timestamp | sim | Horário da origem |

```text
{"eventId":"evt-100","numeroTicket":"T-100","correlationId":"corr-100","recebidoEm":"2026-09-12T10:00:00Z"}
```

#### `Integracao` — outbox de saída

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `id` | UUID | sim | Identificador/event id de saída |
| `tipo` | string | sim | Tipo versionado que seleciona o handler |
| `destino` | string | sim | Destino lógico |
| `payload` | jsonb | sim | Contrato serializado |
| `status` | enum | sim | Pendente, Processando, Concluida ou FalhaDefinitiva |
| `tentativas` | int | sim | Contador de falhas |
| `correlationId` | string | sim | Correlação distribuída |

```text
{"tipo":"ResultadoChecklistEmpresaA.v1","destino":"EmpresaA","status":"Pendente","tentativas":0}
```

#### `ProblemDetails` — envelope de erro

| Código | HTTP | Significado |
| --- | --- | --- |
| `validation_error` | 400 | Entrada inválida |
| `unauthorized` | 401 | Token ausente ou inválido |
| `not_found` | 404 | Recurso inexistente |

```text
{"type":"about:blank","title":"Bad Request","status":400}
```

#### Mapeamento SAP → contrato

| Origem (SAP) | Destino (Cadastro) |
| --- | --- |
| `Tipo` | `Tipo` |
| `Codigo` | `Codigo` |
| `Nome` | `Nome` |
| `CodigoExterno` | `CodigoExterno` |

#### Parâmetros fixos na origem

| API | Parâmetros principais |
| --- | --- |
| **SAP Cadastros** | `CadastrosPath=api/cadastros` |

### Endpoints da API

#### Visão geral

| Método | Rota | Descrição |
| --- | --- | --- |
| GET/POST/PUT/DELETE | `/api/cadastros` | CRUD de cadastros |
| GET | `/api/tickets/{id}` | Consulta ticket e checklist |
| PUT | `/api/tickets/{id}/checklist` | Responde checklist e cria outbox |
| POST | `/api/jobs/sincronizacao-sap` | Agenda command no Hangfire |
| GET | `/health` | Saúde do host |

---

#### `GET /api/cadastros?tipo=Fazenda`

Lista cadastros do tipo informado.

**Parâmetros de consulta**

| Parâmetro | Tipo | Padrão | Regras |
| --- | --- | --- | --- |
| `tipo` | enum | — | obrigatório |

**Respostas**

| Status | Corpo | Quando |
| --- | --- | --- |
| 200 | `Cadastro[]` | consulta válida, inclusive vazia |
| 401 | `ProblemDetails` | usuário não autenticado |

**Exemplo — sucesso**

```http
GET /api/cadastros?tipo=Fazenda
```

```text
[{"tipo":"Fazenda","codigo":"F001","nome":"Fazenda Sul"}]
```

---

#### `PUT /api/tickets/{id}/checklist`

Responde todos os itens e cria a outbox de Empresa A atomicamente.

**Corpo**

| Parâmetro | Tipo | Padrão | Regras |
| --- | --- | --- | --- |
| `respostas` | mapa | — | Item1, Item2 e Item3 obrigatórios |
| `correlationId` | string | — | obrigatório |

**Respostas**

| Status | Corpo | Quando |
| --- | --- | --- |
| 204 | vazio | persistência concluída |
| 400 | `ProblemDetails` | checklist inválido |
| 404 | `ProblemDetails` | ticket inexistente |

**Exemplo — sucesso**

```http
PUT /api/tickets/51c91fec-6602-4821-9af1-73031e6acc5c/checklist
```

```text
{"respostas":{"Item1":"Conforme","Item2":"NaoConforme","Item3":"Conforme"},"correlationId":"corr-100"}
```

> Os demais endpoints seguem os mesmos contratos OpenAPI expostos no Scalar.

---

## Pontos de integração

- SAP usa `HttpClient` com resilience handler padrão e configuração tipada.
- Event Hubs usa producer para saída e `EventProcessorClient` com Blob Storage para checkpoint.
- Entra ID valida bearer tokens na API; tokens não entram em jobs ou eventos.
- PostgreSQL usa outbox, inbox e `FOR UPDATE SKIP LOCKED` para concorrência.

## Abordagem de testes

### Testes de unidade

| ID | Nome do caso de teste | Critérios de aceitação | Resultado esperado |
|----|-----------------------|------------------------|--------------------|
| TU-01 | Ticket possui checklist padrão | CA-02 | três itens esperados |
| TU-02 | Checklist incompleto é rejeitado | CA-03 | exceção de domínio |

### Testes de integração

| ID | Nome do caso de teste | Critérios de aceitação | Resultado esperado |
|----|-----------------------|------------------------|--------------------|
| TI-01 | Health anônimo | CA-04 | HTTP 200 e healthy |
| TI-02 | Regras de dependência | CA-05 | nenhuma referência proibida |

### Testes E2E

| ID | Nome do caso de teste | Critérios de aceitação | Resultado esperado |
|----|-----------------------|------------------------|--------------------|
| E2E-01 | Referência Scalar | CA-08 | UI carrega OpenAPI |

CA-01 é validado pelo build. CA-06 e CA-07 são verificados por inspeção e devem ganhar testes com serviços reais em projetos derivados.

## Sequenciamento do desenvolvimento

### Ordem de construção

1. Criar projetos e dependências.
2. Implementar domínio, aplicação e persistência.
3. Implementar gates, eventos e workers.
4. Implementar Controllers, autenticação e Scalar.
5. Adicionar testes, manifests e documentação.

### Dependências técnicas

- SDK .NET 10 e feed NuGet.
- PostgreSQL, Redis, Event Hubs, Blob Storage e Entra ID para execução completa.

## Monitoramento e observabilidade

Cada host registra falhas com event ids estáveis e correlation id. O endpoint `/health` cobre liveness básica; projetos derivados devem acrescentar readiness das dependências e métricas de lag, duração e retries.

## Considerações técnicas

### Principais decisões

- Controllers oferecem convenções mais explícitas para APIs empresariais extensas.
- Gate identifica saída por fornecedor; SAP também é gate, embora o fluxo funcional seja de leitura.
- Cortex permanece in-process; Hangfire transporta commands entre API e worker.
- Entrega de eventos é pelo menos uma vez, com inbox/outbox e idempotência.

### Riscos conhecidos

- O template não provisiona recursos externos nem migrations específicas do produto.
- Contratos reais de SAP e fornecedores devem substituir os exemplos.
- Health/readiness e observabilidade precisam ser ampliados conforme SLAs do produto.

### Conformidade com o AGENTS.md e as rules

Foram lidos o `AGENTS.md` raiz, `template/AGENTS.md` e todas as rules em `template/.agents/rules/`. A solução preserva direção de dependências, Controllers finos, configuração externa, cancellation tokens, policies, gates isolados e workers independentes.

### Conformidade com skills

Nenhuma skill de projeto em `template/.agents/skills/` é aplicável ao backend .NET desta entrega.

### Arquivos relevantes e dependentes

- `template/backend/Backend.slnx`
- `template/backend/src/Backend.Api/`
- `template/backend/src/Backend.App/`
- `template/backend/src/Backend.Domain/`
- `template/backend/src/Backend.Infra.*/`
- `template/backend/src/Backend.Worker.*/`
- `template/backend/tests/`
- `template/backend/deploy/k8s/`
