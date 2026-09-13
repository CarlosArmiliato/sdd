# Tarefa 2.0: Persistir ActorId nos campos legados de auditoria

## Visão geral

Adaptar a auditoria existente para gravar o `ActorId` nos campos legados `CreatorUsername` e `ModifierUsername`. A solução deve reduzir o impacto de implantação, evitar migração desses campos e preservar a leitura dos registros históricos que ainda contêm e-mail ou username.

<skills>
### Conformidade com skills

As skills disponíveis em `template/.agents/skills` foram avaliadas. Nenhuma delas é aplicável, pois a tarefa é exclusivamente de backend .NET e persistência.
</skills>

<rules>
### Conformidade com o AGENTS.md e as rules

Foram lidos o `AGENTS.md` da raiz, o `template/AGENTS.md` e todas as rules em `template/.agents/rules`. A alteração deve respeitar o mini-monólito, concentrar a persistência em `Backend.Infra`, preservar o domínio sem dependências de infraestrutura e manter cobertura mínima de 80%. A rule de JavaScript/TypeScript não se aplica.
</rules>

<requirements>

- RF1 — Identificar de forma estável o ator responsável por criação e alteração.
- RF2 — Não persistir nome ou e-mail como novo identificador técnico de auditoria.
- RF3 — Preservar compatibilidade de leitura com auditoria histórica.
- RF9 — Auditar operações app-to-app sem deixar o ator nulo.
- RF12 — Auditar chamadas de fornecedores com sua identidade própria.

</requirements>

## Subtarefas

- [x] 2.1 Localizar todos os pontos que preenchem `CreatorUsername` e `ModifierUsername` e centralizar a resolução do valor de auditoria.
- [x] 2.2 Alterar o interceptor ou writer de auditoria para persistir `IUserContext.ActorId` nos campos legados, sem renomear colunas.
- [x] 2.3 Impedir novas gravações de e-mail, nome ou username quando existir uma identidade autenticada.
- [x] 2.4 Implementar leitura tolerante que reconheça valores `v1:*` e preserve valores históricos legados sem reescrita automática.
- [x] 2.5 Garantir que criação e alteração usem o mesmo contrato para atores humanos e aplicações.
- [x] 2.6 Implementar testes unitários e de integração da persistência e da compatibilidade histórica.

## Detalhes de implementação

Seguir as seções “Persistência e compatibilidade da auditoria” e “Estratégia de migração” da TechSpec. Não criar migração para renomear `CreatorUsername` ou `ModifierUsername`; o significado lógico muda para ActorId, mas o schema físico permanece compatível.

## Critérios de aceitação relacionados

- CA-03 — Auditoria humana persiste ActorId sem nome ou e-mail.
- CA-05 — Operações de aplicações internas possuem ator de criação e alteração.
- CA-08 — Operações de fornecedores possuem ator próprio e rastreável.

## Testes da tarefa

### Testes de unidade

- [x] TU-09 — Gravar ActorId nos campos legados de auditoria.
- [x] TU-10 — Preservar auditoria histórica legada.

### Testes de integração

- [x] TI-03 — Persistir auditoria de usuário humano sem PII.

### Testes E2E

Não se aplica diretamente; a comprovação ponta a ponta será executada na tarefa 8.0.

## Arquivos relevantes

- `template/backend/src/Backend.Domain/Common/` — entidades auditáveis e contratos de domínio existentes.
- `template/backend/src/Backend.Infra/Persistence/` — interceptor ou writer de auditoria e mapeamentos EF Core.
- `template/backend/tests/Backend.UnitTests/Auditing/` — testes unitários de auditoria.
- `template/backend/tests/Backend.IntegrationTests/Auditing/` — testes com PostgreSQL.
- `tasks/prd-seguranca-entra/techspec.md` — regras de compatibilidade e migração.
