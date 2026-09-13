Neste diretório você vai encontrar  a seguinte estrutura de pastas
- processo
- template

Processo define um processo de desenvolvimento baseado em sdd (spec driven design)
Você vai encontrar os processos de 1 a 6 que devem ser seguidos em ordem para o seu funcionamento

Em template existem os exemplos, regras, skills, rules e demais conhecimentos técnicos para o desenvolvimento.

## Skills do processo SDD

As skills abaixo estão disponíveis neste projeto. Siga-as nesta ordem quando a
solicitação abranger mais de uma etapa: PRD → TechSpec → tarefas → execução →
QA → revisão. Leia integralmente o `SKILL.md` aplicável antes de agir e use os
templates e as referências indicados pela skill.

- `criar-prd` — `processo/01-criar-prd/SKILL.md` — use para levantar e
  documentar requisitos e escopo de um produto ou funcionalidade em um PRD.
- `criar-techspec` — `processo/02-criar-techspec/SKILL.md` — use para definir
  a especificação técnica de um PRD existente.
- `criar-tasks` — `processo/03-criar-tasks/SKILL.md` — use para decompor um
  PRD com TechSpec em entregas incrementais de implementação.
- `executar-task` — `processo/04-executar-task/SKILL.md` — use para
  implementar a próxima tarefa pendente de uma funcionalidade planejada.
- `executar-qa` — `processo/05-executar-qa/SKILL.md` — use para validar e
  estabilizar a funcionalidade implementada contra o PRD, a TechSpec e as
  tarefas.
- `executar-review` — `processo/06-executar-review/SKILL.md` — use para
  revisar a conformidade do código com as regras do projeto, a TechSpec, as
  tarefas e os testes.
## Ambiente local de desenvolvimento

- O Rancher Desktop está disponível neste ambiente e pode ser usado para simular infraestrutura de desenvolvimento.
- Use seus containers para executar PostgreSQL isolado por worktree quando testes de integração, migrations ou a aplicação exigirem banco real. Cada worktree deve usar nome, volume e porta exclusivos.
- Use o Kubernetes local do Rancher Desktop para validar manifests, workloads e integrações de orquestração quando isso fizer parte da tarefa. Isole os recursos por namespace da worktree.
- Não versione credenciais nem connection strings. Registre portas e processos iniciados, encerre os recursos criados ao final e não remova recursos pertencentes a outra worktree ou ao usuário.
