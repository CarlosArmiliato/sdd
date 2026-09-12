# Documento de Requisitos do Produto (PRD)

## Visão geral

Evoluir o template de backend para um mini-monólito empresarial .NET 10, adequado a sistemas de produção em larga escala com regras complexas e integrações externas. O template deve demonstrar um fluxo agrícola realista, mantendo API e workers no mesmo produto, mas em processos implantáveis e escaláveis separadamente.

## Objetivos

- Oferecer uma estrutura explícita e compilável para API, domínio, aplicação, contratos, infraestrutura e três tipos de worker.
- Demonstrar CRUD de cadastros, sincronização SAP, entrada e saída por Event Hubs e outbox transacional.
- Garantir build sem warnings, testes automatizados passando e dependências arquiteturais verificadas.

## Histórias de usuário

- US1: Como desenvolvedor, quero iniciar um backend empresarial organizado para acrescentar regras de negócio sem acoplar fornecedores ao domínio.
- US2: Como operador, quero executar API, jobs, polling e consumidores separadamente para escalar cada carga no AKS.
- US3: Como usuário autenticado, quero manter cadastros e responder checklists de tickets.
- US4: Como integrador, quero receber tickets e publicar resultados de forma idempotente e auditável.

## Principais funcionalidades

- RF1: Expor CRUD REST de Fazenda, AnoAgricola, Safra e Cultura por Controllers.
- RF2: Sincronizar os cadastros diariamente a partir do SAP por Hangfire.
- RF3: Consumir tickets do Azure Event Hubs com checkpoint durável e idempotência.
- RF4: Criar tickets com checklist padrão de três itens e aceitar Conforme ou NaoConforme.
- RF5: Gravar a resposta e a integração de saída na mesma transação PostgreSQL.
- RF6: Processar a outbox por polling concorrente, retry limitado e handler por tipo de integração.
- RF7: Publicar o resultado da Empresa A no Event Hub.
- RF8: Autenticar com Microsoft Entra ID e oferecer documentação interativa com Scalar.
- RF9: Fornecer manifests KEDA para scale-to-zero dos workers de polling e eventos.

## Critérios de aceitação

- CA-01: Dado o template restaurado, quando a solução for compilada, então todos os projetos compilam sem warnings ou erros.
- CA-02: Dado um ticket novo, quando ele for criado, então contém Item1, Item2 e Item3 sem resposta.
- CA-03: Dado um checklist incompleto, quando for respondido, então a operação é rejeitada.
- CA-04: Dado o endpoint de saúde, quando chamado sem autenticação, então retorna HTTP 200 e estado healthy.
- CA-05: Dada a estrutura de projetos, então Domain não referencia App ou Infra e App não referencia Infra.
- CA-06: Dada uma mensagem processada com sucesso, então seu checkpoint só é atualizado depois do caso de uso local.
- CA-07: Dadas várias réplicas de polling, então uma integração é reservada por apenas uma réplica.
- CA-08: Dado o ambiente de desenvolvimento, então Scalar referencia os endpoints dos Controllers.

## Experiência do usuário

Desenvolvedores interagem com a API pelo Scalar em `/scalar/v1`. Operadores configuram conexões e identidades externamente, implantam hosts separados e observam falhas pelos logs estruturados. Não há interface frontend nova neste escopo.

## Restrições técnicas de alto nível

- .NET 10, Controllers, Cortex.Mediator, FluentValidation, EF Core/PostgreSQL, Redis, Hangfire, Polly, Entra ID e Event Hubs.
- Implantação no AKS com identidade de workload e segredos externos ao repositório.
- Processamento pelo menos uma vez, com idempotência, correlação e checkpoint persistente.

## Fora do escopo

- Implementar regras reais ou credenciais dos fornecedores.
- Provisionar recursos Azure, banco, KEDA ou pipeline CI/CD.
- Implementar frontend ou dashboard Hangfire.
- Garantir exatamente uma vez entre sistemas distribuídos.
