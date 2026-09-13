# Tarefa 4.0: Implementar autorização por policies, permissões e escopos

## Visão geral

Implementar autorização semântica no ASP.NET Core, traduzindo as roles recebidas do Entra ID para perfis, permissões e escopos do catálogo interno. Controllers e casos de uso devem depender de policies ou de `IPermissionContext`, sem verificações diretas e espalhadas de nomes de roles.

<skills>
### Conformidade com skills

As skills disponíveis em `template/.agents/skills` foram avaliadas. Nenhuma delas é aplicável, pois a tarefa é de autorização no backend ASP.NET Core.
</skills>

<rules>
### Conformidade com o AGENTS.md e as rules

Foram lidos o `AGENTS.md` da raiz, o `template/AGENTS.md` e todas as rules em `template/.agents/rules`. Controllers devem permanecer finos, casos de uso ficam em `Backend.App`, nomes de permissões devem ser centralizados e a autorização deve usar policies e handlers testáveis. Não devem existir dependências de ASP.NET Core no domínio. A rule de JavaScript/TypeScript não se aplica.
</rules>

<requirements>

- RF4 — Mapear roles do Entra ID para perfis internos.
- RF5 — Resolver funcionalidades e permissões a partir dos perfis.
- RF6 — Autorizar endpoints e regras de negócio por permissão e escopo.
- RF8 — Aplicar autorização tanto a usuários quanto a aplicações.
- RF11 — Restringir fornecedores às permissões contratadas.

</requirements>

## Subtarefas

- [ ] 4.1 Criar `IPermissionContext` e sua implementação para resolver permissões e escopos do ator atual.
- [ ] 4.2 Criar requirements, policies e `AuthorizationHandler` para permissões funcionais e escopos.
- [ ] 4.3 Centralizar nomes de policies e permissões, incluindo o exemplo de Cadastros, Tickets, Jobs e resolução de identidades.
- [ ] 4.4 Aplicar policies nos endpoints e manter os Controllers responsáveis apenas pela orquestração HTTP.
- [ ] 4.5 Expor consultas semânticas como `HasPermission` para regras que precisem autorizar dentro dos casos de uso.
- [ ] 4.6 Padronizar respostas 401 para falha de autenticação e 403 para identidade válida sem autorização.
- [ ] 4.7 Implementar testes unitários do handler e testes de integração de permissão e escopo.

## Detalhes de implementação

Seguir as seções “Autorização baseada em policies”, “Permissões e escopos” e “Contratos da aplicação” da TechSpec. Não implementar condicionais como `Roles.Contains("Admin")` na camada de negócio. A resolução deve ocorrer por meio do catálogo interno criado na tarefa 3.0.

## Critérios de aceitação relacionados

- CA-01 — Usuário autorizado acessa somente as funcionalidades e escopos mapeados.
- CA-02 — Usuário sem permissão ou fora do escopo recebe negação consistente.
- CA-06 — Aplicação interna sem app role ou permissão interna é rejeitada.
- CA-09 — Fornecedor fora do contrato não acessa o recurso.

## Testes da tarefa

### Testes de unidade

- [ ] TU-06 — Autorizar policy semântica por permissão e escopo.

### Testes de integração

- [ ] TI-01 — Autorizar usuário com permissão e escopo válidos.
- [ ] TI-02 — Negar usuário sem permissão ou fora do escopo.

### Testes E2E

Não se aplica diretamente; os mesmos fluxos serão exercitados com tokens reais na tarefa 8.0.

## Arquivos relevantes

- `template/backend/src/Backend.App/Authorization/IPermissionContext.cs` — contrato semântico de autorização.
- `template/backend/src/Backend.API/Authorization/` — policies, requirements e handlers.
- `template/backend/src/Backend.API/Controllers/CadastrosController.cs` — endpoint exemplo protegido.
- `template/backend/tests/Backend.UnitTests/Authorization/` — testes de handlers e contextos.
- `template/backend/tests/Backend.IntegrationTests/Authorization/` — testes HTTP de autorização.
- `tasks/prd-seguranca-entra/techspec.md` — matriz de permissões e comportamento HTTP.
