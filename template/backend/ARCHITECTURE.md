# Arquitetura de referência do backend

## Propósito

O backend é um mini-monólito modular .NET 10 para sistemas empresariais de produção em larga escala. Um sistema possui um frontend e um backend, sem dividi-los em microserviços. API e workers são processos do mesmo backend e podem ser escalados separadamente no AKS.

## Hosts

| Host | Responsabilidade | Escala |
| --- | --- | --- |
| `Backend.Api` | API HTTP baseada em Controllers | Por tráfego HTTP |
| `Backend.Worker.Hangfire` | Jobs agendados e enfileirados | Por fila de jobs |
| `Backend.Worker.Polling` | Trabalho interno pendente | KEDA, inclusive scale-to-zero quando a fonte for observável externamente |
| `Backend.Worker.EventHub` | Consumo de eventos de integração | KEDA por backlog do Event Hub |

Os hosts compartilham `Backend.App`, `Backend.Domain`, `Backend.Contracts` e os projetos de infraestrutura. Eles não se referenciam entre si.

## Camadas

```text
Controller ou Worker
        ↓
Backend.App: Command, Query, Handler e Behavior
        ├──→ Backend.Domain: regras e modelo de negócio
        └──→ portas → Backend.Infra.*: persistência, cache, jobs e integrações
```

`Backend.App` contém as abstrações usadas pelos casos de uso. `Backend.Infra.*` implementa essas abstrações. O host correspondente registra implementações e handlers no contêiner de dependências.

## API

Controllers são a entrada HTTP obrigatória. Eles recebem contratos HTTP, aplicam autenticação e autorização, acionam o caso de uso e retornam a resposta HTTP. Não devem conter regras de negócio, acesso a banco, retry ou chamadas diretas a fornecedores.

Autenticação usa OAuth 2.0/OpenID Connect com Microsoft Entra ID. Autorização é feita com policies. Erros são retornados como Problem Details.

## Mediator e processamento em segundo plano

Cortex.Mediator realiza o despacho dentro do processo que possui o handler registrado. Uma chamada da API não executa automaticamente um handler carregado somente no worker.

Para execução assíncrona, o fluxo é:

```text
Controller → Command em Backend.App → IBackgroundCommandScheduler
→ Hangfire → Backend.Worker.Hangfire → Cortex.Mediator → handler do worker
```

O comando HTTP agenda o trabalho e retorna `202 Accepted` com o identificador do job. O comando serializado contém somente dados simples, é idempotente e não contém `HttpContext`, entidades EF Core ou tokens de usuário. O handler de `SincronizarCadastrosSapCommand` existe apenas em `Backend.Worker.Hangfire`.

## Eventos de integração

Eventos de domínio são internos ao mini-monólito. Eventos de integração são contratos versionáveis em `Backend.Contracts/Events` e são transportados pelo Azure Event Hubs.

```text
Domain Event → handler em Backend.App → Backend.Infra.EventHub → Event Hubs
Event Hubs → Backend.Worker.EventHub → Command em Backend.App → Domain
```

Consumidores persistem checkpoints, processam mensagens de forma idempotente e propagam correlation ID. O worker de eventos utiliza grupo de consumidores próprio por fluxo.

## Exemplo de negócio incluído

- `Fazenda`, `AnoAgricola`, `Safra` e `Cultura` usam o agregado `Cadastro`, com CRUD pelos Controllers e sincronização diária pelo gate SAP.
- `TicketRecebidoV1` chega pelo Event Hub. O consumidor usa Blob Storage para checkpoint e a tabela `EventosRecebidos` para idempotência.
- O ticket nasce com `Item1`, `Item2` e `Item3`. A resposta do checklist e a criação de `Integracoes` são persistidas na mesma transação.
- O worker de polling reserva a outbox com `FOR UPDATE SKIP LOCKED`, evitando que réplicas processem a mesma linha simultaneamente.
- O handler `ResultadoChecklistEmpresaAHandler` transforma a linha de outbox no contrato `ResultadoChecklistEmpresaAV1` e publica no Event Hub.
- Falhas voltam ao estado pendente com backoff exponencial; leases expirados são recuperados para suportar interrupções do pod.

## Persistência e integrações

- `Backend.Infra.Postgres` concentra EF Core, migrations, mapeamentos code first e repositórios.
- `Backend.Infra.Redis` concentra cache distribuído, locks e coordenação.
- `Backend.Infra.Hangfire` concentra o transporte e a configuração de jobs.
- `Backend.Infra.Gate.<Fornecedor>` representa uma integração de saída. Cada projeto contém clientes, contratos externos, mappers, autenticação e políticas Polly específicas do fornecedor.

Integrações usam timeouts, retries, circuit breakers, telemetria e idempotência. O estado de cada processamento deve permitir auditoria e reprocessamento seguro.

## Regras operacionais

- Configure health checks de liveness e readiness para cada host e suas dependências relevantes.
- Use identidade de workload no AKS para acessar recursos Azure; não armazene segredos no repositório.
- Workers devem suportar cancelamento e desligamento gracioso.
- O worker de polling só pode escalar a zero se o KEDA consultar uma métrica ou fila externa ao pod.
- Aplique outbox quando a persistência de uma alteração de domínio e a publicação de evento precisarem de consistência operacional.
