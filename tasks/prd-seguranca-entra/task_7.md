# Tarefa 7.0: Auditar workers com ActorContext por execução

## Visão geral

Garantir que cada job agendado ou mensagem processada por worker possua um ator técnico desde o início da execução. Quando não houver usuário originador, o worker deve representar a si mesmo pelo ActorId da sua aplicação; quando houver, ambos os contextos devem ser preservados para rastreabilidade.

<skills>
### Conformidade com skills

As skills disponíveis em `template/.agents/skills` foram avaliadas. Nenhuma delas é aplicável, pois a tarefa é de processamento assíncrono e auditoria no backend .NET.
</skills>

<rules>
### Conformidade com o AGENTS.md e as rules

Foram lidos o `AGENTS.md` da raiz, o `template/AGENTS.md` e todas as rules em `template/.agents/rules`. Workers permanecem em projetos separados, reutilizando casos de uso de `Backend.App`; mensagens não transportam tokens; contexto de execução deve ser scoped, seguro para concorrência e limpo ao final. Logs estruturados devem incluir correlação sem secrets ou PII. A rule de JavaScript/TypeScript não se aplica.
</rules>

<requirements>

- RF1 — Toda alteração deve possuir um ator estável.
- RF7 — Workers são identificados como aplicações internas.
- RF9 — Operações assíncronas não deixam auditoria nula.
- RF12 — Operações originadas por integração preservam a identidade relevante para auditoria.

</requirements>

## Subtarefas

- [ ] 7.1 Criar `IActorContextAccessor` com armazenamento scoped por execução e sem estado global compartilhado.
- [ ] 7.2 Implementar `SystemUserContext` para representar o worker pela identidade de sua workload/service principal.
- [ ] 7.3 Inicializar o `ActorContext` técnico no início de jobs Hangfire, polling e consumidores Event Hub.
- [ ] 7.4 Propagar apenas o snapshot mínimo de identidade originadora e o correlation ID em comandos ou eventos, nunca o token.
- [ ] 7.5 Aplicar a regra de auditoria `Originator ?? Executor`, usando `v1:entra:app:{tid}:{oid}` quando não houver originador.
- [ ] 7.6 Limpar o accessor em bloco `finally` para evitar vazamento entre jobs reutilizados ou concorrentes.
- [ ] 7.7 Implementar testes unitários e de integração do ator técnico, da precedência do originador e do isolamento concorrente.

## Detalhes de implementação

Seguir as seções “Workers e operações assíncronas”, “Propagação de contexto” e “Auditoria” da TechSpec. Em ambiente local ou teste sem identidade Entra, usar somente o fallback explícito `v1:system:{host}:{processo}`. O valor de auditoria não pode ser nulo.

## Critérios de aceitação relacionados

- CA-05 — Worker interno possui identidade técnica estável e auditável.
- CA-08 — Uma operação originada por fornecedor preserva rastreabilidade do originador quando aplicável.

## Testes da tarefa

### Testes de unidade

- [ ] TU-11 — Selecionar o ator de auditoria do worker.
- [ ] TU-12 — Limpar o contexto ao finalizar o job.

### Testes de integração

- [ ] TI-13 — Auditar execução de worker com ator técnico.
- [ ] TI-15 — Isolar ActorContext entre jobs concorrentes.

### Testes E2E

O comportamento da identidade app-only será exercitado por E2E-02 na tarefa 8.0.

## Arquivos relevantes

- `template/backend/src/Backend.App/Identity/IActorContextAccessor.cs` — novo contrato de contexto por execução.
- `template/backend/src/Backend.App/Identity/SystemUserContext.cs` — identidade técnica do worker.
- `template/backend/src/Backend.Worker.Hangfire/` — inicialização e limpeza do contexto em jobs.
- `template/backend/src/Backend.Worker.Polling/` — inicialização e limpeza do contexto no polling.
- `template/backend/src/Backend.Worker.EventHub/` — propagação de originador e executor.
- `template/backend/tests/Backend.UnitTests/Workers/` — testes de seleção e limpeza.
- `template/backend/tests/Backend.IntegrationTests/Workers/` — testes de auditoria e concorrência.
- `tasks/prd-seguranca-entra/techspec.md` — regra `Originator ?? Executor` e limites de propagação.
