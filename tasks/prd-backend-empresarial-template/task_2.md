# Tarefa 2.0: Implementar fluxo de negócio e integrações

## Visão geral

Implementar cadastros, tickets, checklist, inbox/outbox, SAP e Empresa A.

<skills>
### Conformidade com skills

Nenhuma skill de projeto aplicável.
</skills>

<rules>
### Conformidade com o AGENTS.md e as rules

Domínio e aplicação permanecem independentes de EF Core, Azure, Hangfire e fornecedores.
</rules>

<requirements>
RF1, RF2, RF3, RF4, RF5, RF6 e RF7.
</requirements>

## Subtarefas

- [x] 2.1 Modelar Cadastro, Ticket, ChecklistItem e Integracao.
- [x] 2.2 Implementar casos de uso e validação.
- [x] 2.3 Implementar PostgreSQL, inbox e outbox transacional.
- [x] 2.4 Implementar gates SAP, Empresa A e extensão Empresa B.

## Detalhes de implementação

Consulte “Modelos de dados” e “Pontos de integração” na TechSpec.

## Critérios de aceitação relacionados

- CA-02
- CA-03
- CA-06
- CA-07

## Testes da tarefa

### Testes de unidade

- [x] TU-01 — Ticket possui checklist padrão
- [x] TU-02 — Checklist incompleto é rejeitado

### Testes de integração

Não aplicável no template sem serviços externos.

### Testes E2E

Não aplicável.

## Arquivos relevantes

- `template/backend/src/Backend.Domain/`
- `template/backend/src/Backend.App/`
- `template/backend/src/Backend.Infra.Postgres/`
- `template/backend/src/Backend.Infra.Gate.*/`
