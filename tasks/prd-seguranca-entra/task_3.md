# Tarefa 3.0: Persistir catálogo de perfis, permissões, escopos e identidades de aplicação

## Visão geral

Criar no PostgreSQL o catálogo interno que desacopla roles do Entra ID das regras de negócio. O catálogo deve mapear roles externas para perfis, permissões funcionais e escopos, além de registrar e controlar identidades de aplicações internas e fornecedores.

<skills>
### Conformidade com skills

As skills disponíveis em `template/.agents/skills` foram avaliadas. Nenhuma delas é aplicável, pois a tarefa trata de domínio, EF Core e PostgreSQL no backend.
</skills>

<rules>
### Conformidade com o AGENTS.md e as rules

Foram lidos o `AGENTS.md` da raiz, o `template/AGENTS.md` e todas as rules em `template/.agents/rules`. Entidades e regras invariantes devem permanecer em `Backend.Domain`; implementações de EF Core ficam em `Backend.Infra`; configurações de entidades devem ser explícitas; migrations devem ser versionadas e os testes devem usar infraestrutura real quando necessário. A rule de JavaScript/TypeScript não se aplica.
</rules>

<requirements>

- RF4 — Mapear roles do Entra ID para perfis internos.
- RF5 — Associar perfis a funcionalidades e permissões.
- RF6 — Restringir acesso por permissões e escopos internos.
- RF8 — Manter cadastro de identidades de aplicação e seu tipo.
- RF10 — Isolar cada fornecedor em um App Registration próprio.
- RF11 — Associar permissões específicas à integração do fornecedor.
- RF12 — Identificar e auditar o fornecedor responsável.
- RF13 — Revogar um fornecedor sem afetar os demais.

</requirements>

## Subtarefas

- [ ] 3.1 Modelar `PerfilAcesso`, `Permissao`, `PerfilPermissao`, `MapeamentoRolePerfil`, `MapeamentoRoleEscopo` e `IdentidadeAplicacao` no domínio.
- [ ] 3.2 Definir invariantes para chaves, tipo de identidade, tenant, client ID, object ID, status e escopos permitidos.
- [ ] 3.3 Criar mapeamentos EF Core, índices e restrições de unicidade e integridade referencial.
- [ ] 3.4 Criar migration PostgreSQL para o catálogo de autorização e identidades de aplicação.
- [ ] 3.5 Implementar repositórios e `IAuthorizationCatalog` com consultas eficientes para autorização.
- [ ] 3.6 Disponibilizar seed idempotente para permissões e perfis mínimos do sistema exemplo.
- [ ] 3.7 Implementar testes unitários e de integração do catálogo, dos escopos e do isolamento de fornecedores.

## Detalhes de implementação

Seguir as seções “Modelo de autorização interno”, “Modelo de dados” e “Persistência PostgreSQL” da TechSpec. Não armazenar tokens nem segredos. A identidade do fornecedor deve usar a combinação validada de tenant, object ID/client ID e cadastro ativo, e não apenas o texto de uma role.

## Critérios de aceitação relacionados

- CA-01 — Roles são convertidas em perfis, permissões e escopos internos.
- CA-02 — A ausência de permissão ou escopo resulta em negação.
- CA-07 — Cada fornecedor possui identidade exclusiva e isolada.
- CA-08 — O fornecedor é identificável para auditoria.
- CA-09 — Fornecedor sem autorização contratada é rejeitado.
- CA-10 — A revogação de um fornecedor não afeta outros.

## Testes da tarefa

### Testes de unidade

- [ ] TU-04 — Resolver role para perfil e permissão.
- [ ] TU-05 — Aplicar escopo de filial.
- [ ] TU-07 — Resolver identidade de fornecedor.
- [ ] TU-08 — Rejeitar fornecedor inativo ou divergente.

### Testes de integração

- [ ] TI-08 — Garantir identidade exclusiva por fornecedor.

### Testes E2E

Não se aplica diretamente; os cenários completos de fornecedores serão cobertos na tarefa 8.0.

## Arquivos relevantes

- `template/backend/src/Backend.Domain/Authorization/` — novas entidades e invariantes do catálogo.
- `template/backend/src/Backend.App/Authorization/IAuthorizationCatalog.cs` — abstração de consulta.
- `template/backend/src/Backend.Infra/Persistence/Configurations/` — configurações EF Core.
- `template/backend/src/Backend.Infra/Persistence/Migrations/` — migration do catálogo.
- `template/backend/tests/Backend.UnitTests/Authorization/` — testes de regras do catálogo.
- `template/backend/tests/Backend.IntegrationTests/Authorization/` — testes PostgreSQL.
- `tasks/prd-seguranca-entra/techspec.md` — modelo lógico e restrições.
