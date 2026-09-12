# Tarefa 4.0: Validar, documentar e integrar

## Visão geral

Executar build, testes, QA, revisão, documentação e integração na branch principal.

<skills>
### Conformidade com skills

Nenhuma skill de projeto aplicável.
</skills>

<rules>
### Conformidade com o AGENTS.md e as rules

Build e testes da solução são obrigatórios; a entrega não deve versionar segredos.
</rules>

<requirements>
Todos os requisitos RF1 a RF9.
</requirements>

## Subtarefas

- [x] 4.1 Compilar sem warnings.
- [x] 4.2 Executar testes automatizados.
- [x] 4.3 Validar Scalar em ambiente isolado.
- [x] 4.4 Gerar QA e revisão.
- [x] 4.5 Commitar e integrar na branch principal.

## Detalhes de implementação

Consulte “Abordagem de testes” e “Monitoramento e observabilidade” na TechSpec.

## Critérios de aceitação relacionados

- CA-01
- CA-04
- CA-05
- CA-08

## Testes da tarefa

### Testes de unidade

- [x] TU-01 — Ticket possui checklist padrão
- [x] TU-02 — Checklist incompleto é rejeitado

### Testes de integração

- [x] TI-01 — Health anônimo
- [x] TI-02 — Regras de dependência

### Testes E2E

- [x] E2E-01 — Referência Scalar

## Arquivos relevantes

- `template/backend/README.md`
- `tasks/prd-backend-empresarial-template/qa.md`
- `tasks/prd-backend-empresarial-template/codereview.md`
