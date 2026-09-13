# Tarefa 8.0: Provisionar o ambiente e validar a solução ponta a ponta

## Visão geral

Preparar o tenant e a infraestrutura do sistema exemplo, adicionar observabilidade e validar os fluxos completos de usuário, aplicação interna e fornecedor. A entrega deve comprovar todos os critérios de aceitação com identidades reais de teste, sem credenciais inseguras no repositório.

<skills>
### Conformidade com skills

As skills disponíveis em `template/.agents/skills` foram avaliadas. Nenhuma delas é aplicável, pois a tarefa envolve infraestrutura, documentação e validação de backend, sem interface React ou atividade visual.
</skills>

<rules>
### Conformidade com o AGENTS.md e as rules

Foram lidos o `AGENTS.md` da raiz, o `template/AGENTS.md` e todas as rules em `template/.agents/rules`. Segredos não podem ser versionados; ambientes devem usar configuração externa e identidades gerenciadas; testes devem seguir a pirâmide definida, usar xUnit e manter cobertura mínima de 80%; logs devem ser estruturados e correlacionáveis. A rule de JavaScript/TypeScript não se aplica.
</rules>

<requirements>

- RF1 a RF14 — Validar integralmente identidade, autorização, auditoria, integrações internas, fornecedores e segurança de credenciais.

</requirements>

## Subtarefas

- [ ] 8.1 Criar scripts ou IaC idempotente para a API, app roles, permissões, consentimentos e identidades de workload do ambiente de teste.
- [ ] 8.2 Provisionar uma identidade de usuário, uma aplicação interna e pelo menos dois fornecedores isolados, usando certificados ou federação para app-only.
- [ ] 8.3 Configurar variáveis e referências seguras para CI/CD e ambiente, sem client secrets em arquivos ou logs.
- [ ] 8.4 Adicionar logs, métricas e tracing para tipo do ator, ActorId, decisão de autorização, correlation ID e falhas de autenticação, respeitando minimização de dados.
- [ ] 8.5 Executar o teste de arquitetura das dependências entre projetos e corrigir violações.
- [ ] 8.6 Implementar e executar os quatro cenários E2E com tokens reais do tenant de testes.
- [ ] 8.7 Executar build, testes unitários e de integração, E2E e coleta de cobertura, mantendo no mínimo 80%.
- [ ] 8.8 Documentar onboarding, rotação, revogação, troubleshooting e rollback operacional.

## Detalhes de implementação

Seguir as seções “Infraestrutura e ambientes”, “Observabilidade”, “Estratégia de testes”, “Rollout e rollback” e “Segurança de credenciais” da TechSpec. Artefatos de provisionamento devem ser repetíveis e produzir apenas referências a credenciais externas; certificados privados e tokens nunca devem ser gravados no repositório.

## Critérios de aceitação relacionados

- CA-01 — Autorização de usuário por permissão e escopo.
- CA-02 — Negação de usuário sem permissão ou escopo.
- CA-03 — Auditoria humana sem PII.
- CA-04 — Resolução posterior da identidade humana.
- CA-05 — Identificação e auditoria de aplicação interna.
- CA-06 — Negação de aplicação interna sem autorização.
- CA-07 — Identidade exclusiva por fornecedor.
- CA-08 — Identificação e auditoria do fornecedor.
- CA-09 — Restrição às permissões contratadas.
- CA-10 — Revogação isolada.
- CA-11 — Autenticação app-only sem secret compartilhado ou de longa duração.

## Testes da tarefa

### Testes de unidade

Executar novamente toda a suíte unitária criada nas tarefas anteriores e validar a cobertura consolidada.

### Testes de integração

- [ ] TI-14 — Validar as dependências arquiteturais entre projetos.

### Testes E2E

- [ ] E2E-01 — Executar fluxo delegado de usuário com autorização, auditoria e resolução no Graph.
- [ ] E2E-02 — Executar fluxo app-only de aplicação interna autorizada e negada.
- [ ] E2E-03 — Executar fluxo de fornecedor com certificado, autorização e auditoria.
- [ ] E2E-04 — Revogar um fornecedor e comprovar que outro permanece ativo.

## Arquivos relevantes

- `template/infra/` — scripts ou IaC para Entra ID, workloads e ambiente de teste.
- `template/backend/tests/Backend.ArchitectureTests/` — testes das fronteiras arquiteturais.
- `template/backend/tests/Backend.E2ETests/` — cenários com o tenant de testes.
- `template/docs/security/` — runbooks de onboarding, rotação, revogação e rollback.
- `template/.github/workflows/` — pipeline de build, testes, cobertura e E2E, se aplicável ao repositório.
- `tasks/prd-seguranca-entra/techspec.md` — estratégia completa de implantação e validação.
