# Tarefa 1.0: Normalizar identidade e claims em ActorContext

## Visão geral

Criar o modelo canônico de ator e a camada responsável por transformar claims de tokens delegados e app-only em uma identidade estável. Esta tarefa estabelece a base compartilhada por auditoria, autorização, integrações e workers, sem persistir nome ou e-mail como identificador.

<skills>
### Conformidade com skills

As skills disponíveis em `template/.agents/skills` foram avaliadas. Nenhuma delas é aplicável, pois a tarefa é exclusivamente de backend .NET e não envolve React nem trabalho visual.
</skills>

<rules>
### Conformidade com o AGENTS.md e as rules

Foram lidos o `AGENTS.md` da raiz, o `template/AGENTS.md` e todas as rules em `template/.agents/rules`. A implementação deve manter abstrações em `Backend.App`, evitar dependências de ASP.NET Core, EF Core ou Azure em `Backend.Domain`, usar configuração tipada, DI e testes xUnit. A rule de JavaScript/TypeScript não se aplica a esta tarefa.
</rules>

<requirements>

- RF1 — Identificar de forma estável o ator responsável por criação e alteração.
- RF2 — Não persistir nome ou e-mail como novo identificador técnico de auditoria.
- RF7 — Identificar aplicações internas em chamadas app-to-app.
- RF8 — Distinguir usuários, aplicações internas e fornecedores.

</requirements>

## Subtarefas

- [x] 1.1 Criar os tipos `ActorContext`, `ActorType` e os value objects necessários para tenant, objeto e aplicação.
- [x] 1.2 Implementar a composição e validação do `ActorId` versionado para usuários, aplicações e fallback local/teste.
- [x] 1.3 Evoluir `IUserContext` para expor `ActorId`, tipo do ator, tenant, object ID, client ID e roles, preservando compatibilidade temporária com `Username`.
- [x] 1.4 Implementar `ActorContextFactory` e `HttpUserContext` para normalizar claims delegadas e app-only, rejeitando identidades incompletas ou ambíguas.
- [x] 1.5 Registrar serviços e opções com DI e validação no startup.
- [x] 1.6 Implementar os testes unitários de normalização e rejeição de claims.

## Detalhes de implementação

Seguir as seções “Modelo de identidade e ActorId”, “Claims e normalização do ator” e “Configuração e injeção de dependência” da TechSpec. O formato persistido deve permanecer canônico e versionado: usuário `v1:entra:user:{tid}:{oid}`, aplicação `v1:entra:app:{tid}:{oid}` e fallback controlado `v1:system:{host}:{processo}`.

## Critérios de aceitação relacionados

- CA-03 — Auditoria humana persiste um identificador estável sem PII.
- CA-04 — O identificador de usuário contém dados suficientes para resolução posterior.
- CA-05 — Aplicações internas são identificadas de forma estável.
- CA-06 — Tokens app-only inválidos ou sem permissão são rejeitados.
- CA-08 — Fornecedores podem ser auditados como identidades de aplicação.
- CA-09 — Identidades incompletas ou fora do contrato são rejeitadas.

## Testes da tarefa

### Testes de unidade

- [x] TU-01 — Criar ActorId de usuário.
- [x] TU-02 — Criar ActorId de aplicação.
- [x] TU-03 — Rejeitar claims incompletas ou ambíguas.

### Testes de integração

Não se aplica diretamente; a integração com autenticação e autorização será coberta nas tarefas 4.0 e 5.0.

### Testes E2E

Não se aplica diretamente; os fluxos completos serão cobertos na tarefa 8.0.

## Arquivos relevantes

- `template/backend/src/Backend.App/Abstractions/IUserContext.cs` — contrato de identidade atual a evoluir.
- `template/backend/src/Backend.App/Identity/ActorContext.cs` — novo modelo canônico do ator.
- `template/backend/src/Backend.App/Identity/ActorContextFactory.cs` — nova normalização de claims.
- `template/backend/src/Backend.API/Identity/HttpUserContext.cs` — contexto HTTP baseado no ator normalizado.
- `template/backend/tests/Backend.UnitTests/Identity/` — testes unitários de identidade.
- `tasks/prd-seguranca-entra/techspec.md` — decisões técnicas e formatos canônicos.
