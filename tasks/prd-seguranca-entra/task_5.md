# Tarefa 5.0: Habilitar autenticação app-only para aplicações internas e fornecedores

## Visão geral

Configurar a API para autenticar integrações máquina a máquina com tokens app-only do Entra ID. Aplicações internas e fornecedores devem ser identificados por seu próprio service principal, validados contra o catálogo local e autorizados por app roles e permissões específicas, sem compartilhar credenciais com a API ou com outros fornecedores.

<skills>
### Conformidade com skills

As skills disponíveis em `template/.agents/skills` foram avaliadas. Nenhuma delas é aplicável, pois a tarefa é de segurança e autenticação no backend .NET.
</skills>

<rules>
### Conformidade com o AGENTS.md e as rules

Foram lidos o `AGENTS.md` da raiz, o `template/AGENTS.md` e todas as rules em `template/.agents/rules`. Configuração do Entra deve usar options tipadas e variáveis de ambiente, sem secrets versionados. A validação deve ocorrer na borda HTTP e o domínio não pode depender de bibliotecas do Azure ou ASP.NET Core. A rule de JavaScript/TypeScript não se aplica.
</rules>

<requirements>

- RF7 — Autenticar aplicações internas em integrações REST app-to-app.
- RF8 — Identificar a aplicação chamadora por identidade própria.
- RF9 — Auditar operações app-to-app com ator técnico.
- RF10 — Usar um App Registration específico por fornecedor e fronteira de integração.
- RF11 — Atribuir permissões específicas a cada fornecedor.
- RF12 — Identificar o fornecedor responsável por cada chamada.
- RF13 — Revogar fornecedores de forma isolada.
- RF14 — Não depender de client secret compartilhado ou de longa duração.

</requirements>

## Subtarefas

- [ ] 5.1 Configurar autenticação JWT Bearer para validar issuer, audience, tenant e versão dos tokens delegados e app-only.
- [ ] 5.2 Classificar tokens app-only por `idtyp`, `oid`, `azp`/`appid` e `roles`, rejeitando tokens sem identidade inequívoca.
- [ ] 5.3 Validar a aplicação chamadora contra `IdentidadeAplicacao`, incluindo tipo, status, tenant, IDs e permissões contratadas.
- [ ] 5.4 Definir e documentar app roles da API para aplicações internas e fornecedores, com consentimento administrativo controlado.
- [ ] 5.5 Documentar o onboarding de fornecedor: ClientId da API, tenant e endpoint de token, scope `.default` e credencial própria baseada em certificado ou federação.
- [ ] 5.6 Implementar revogação lógica e garantir que ela afete somente a identidade selecionada.
- [ ] 5.7 Implementar testes de integração de aplicações internas, fornecedores, permissões, revogação e ausência de client secret compartilhado.

## Detalhes de implementação

Seguir as seções “Autenticação app-only”, “Integrações internas”, “Integrações de fornecedores” e “Segurança de credenciais” da TechSpec. O ClientId é identificador público, não credencial. Cada fornecedor deve possuir App Registration próprio e manter sua chave privada; o sistema exemplo não deve criar nem distribuir secret compartilhado da API.

## Critérios de aceitação relacionados

- CA-05 — Aplicação interna autenticada é identificada e auditada.
- CA-06 — Aplicação interna sem app role ou permissão é rejeitada.
- CA-07 — Cada fornecedor usa identidade exclusiva.
- CA-08 — Fornecedor autorizado é identificado e auditado.
- CA-09 — Fornecedor fora do contrato é rejeitado.
- CA-10 — Revogação é isolada por fornecedor.
- CA-11 — O fluxo recomendado não usa client secret compartilhado ou de longa duração.

## Testes da tarefa

### Testes de unidade

Os testes da normalização do ator e do cadastro de fornecedores estão nas tarefas 1.0 e 3.0.

### Testes de integração

- [ ] TI-06 — Autorizar aplicação interna app-only.
- [ ] TI-07 — Rejeitar aplicação interna sem app role.
- [ ] TI-09 — Autorizar e auditar fornecedor válido.
- [ ] TI-10 — Rejeitar fornecedor fora do contrato.
- [ ] TI-11 — Revogar fornecedor sem afetar os demais.
- [ ] TI-12 — Validar configuração sem client secret compartilhado.

### Testes E2E

Não se aplica nesta tarefa; tokens e identidades reais do tenant de testes serão exercitados na tarefa 8.0.

## Arquivos relevantes

- `template/backend/src/Backend.API/Identity/` — configuração JWT e normalização da identidade autenticada.
- `template/backend/src/Backend.API/Authorization/` — políticas para app roles e permissões internas.
- `template/backend/src/Backend.Infra/Persistence/` — consulta das identidades de aplicação.
- `template/backend/tests/Backend.IntegrationTests/Authentication/` — cenários app-only.
- `template/docs/security/supplier-onboarding.md` — novo guia de integração de fornecedores.
- `tasks/prd-seguranca-entra/techspec.md` — fluxo OAuth 2.0 e controles de segurança.
