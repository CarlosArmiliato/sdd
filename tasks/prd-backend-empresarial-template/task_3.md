# Tarefa 3.0: Implementar hosts, API e escalabilidade

## Visão geral

Compor API e workers independentes, autenticação, Scalar, Hangfire, Event Hubs e KEDA.

<skills>
### Conformidade com skills

Nenhuma skill de projeto aplicável.
</skills>

<rules>
### Conformidade com o AGENTS.md e as rules

Controllers e hosts são finos; autorização usa policy; opções são tipadas; segredos ficam externos.
</rules>

<requirements>
RF2, RF3, RF6, RF7, RF8 e RF9.
</requirements>

## Subtarefas

- [x] 3.1 Implementar Controllers, Entra ID e Scalar.
- [x] 3.2 Implementar ponte Hangfire para handlers Cortex do worker.
- [x] 3.3 Implementar workers EventHub e Polling.
- [x] 3.4 Adicionar checkpoint Blob e manifests KEDA.

## Detalhes de implementação

Consulte “Visão dos componentes”, “Endpoints da API” e “Pontos de integração” na TechSpec.

## Critérios de aceitação relacionados

- CA-04
- CA-06
- CA-07
- CA-08

## Testes da tarefa

### Testes de unidade

Não aplicável.

### Testes de integração

- [x] TI-01 — Health anônimo

### Testes E2E

- [x] E2E-01 — Referência Scalar

## Arquivos relevantes

- `template/backend/src/Backend.Api/`
- `template/backend/src/Backend.Worker.*/`
- `template/backend/src/Backend.Infra.EventHub/`
- `template/backend/src/Backend.Infra.Hangfire/`
- `template/backend/deploy/k8s/`
