# Tarefa 1.0: Estruturar solução e dependências

## Visão geral

Criar os projetos físicos e impor a direção de dependências do mini-monólito.

<skills>
### Conformidade com skills

Nenhuma skill de projeto aplicável.
</skills>

<rules>
### Conformidade com o AGENTS.md e as rules

AGENTS e rules lidos; nomes, hosts e dependências seguem `folder-structure.md` e `dotnet.md`.
</rules>

<requirements>
RF1, RF8 e RF9.
</requirements>

## Subtarefas

- [x] 1.1 Criar projetos e solução agrupada.
- [x] 1.2 Centralizar versões de pacotes.
- [x] 1.3 Adicionar testes de arquitetura.

## Detalhes de implementação

Consulte “Arquitetura do sistema” e “Principais decisões” na TechSpec.

## Critérios de aceitação relacionados

- CA-01
- CA-05

## Testes da tarefa

### Testes de unidade

Não aplicável.

### Testes de integração

- [x] TI-02 — Regras de dependência

### Testes E2E

Não aplicável.

## Arquivos relevantes

- `template/backend/Backend.slnx`
- `template/backend/Directory.Packages.props`
- `template/backend/tests/Backend.ArchitectureTests/`
