# PRD — Template de infraestrutura Azure para aplicações EmpresaX

## 1. Visão geral

A EmpresaX possui templates para criação de projetos de backend, frontend e firmware, mas ainda não dispõe de um template equivalente para preparar a infraestrutura Azure necessária a uma nova aplicação. Hoje, essa ausência dificulta a repetibilidade entre ambientes e exige que desenvolvedores e a equipe de infraestrutura reconstruam ou descrevam manualmente recursos que seguem um padrão conhecido.

Este produto deve disponibilizar um template reutilizável de infraestrutura como código para provisionar, de forma idempotente e sem interação humana, os recursos Azure esperados para uma aplicação comum. Cada execução receberá o ambiente como parâmetro e atuará somente sobre esse ambiente.

O template deve se integrar ao modelo de trabalho dos templates de backend e frontend existentes. Desenvolvedores poderão aplicá-lo diretamente em DEV; para QA e PRD, o mesmo conjunto de arquivos e parâmetros deverá poder ser entregue à equipe de infraestrutura para revisão e aplicação.

## 2. Problema

Não existe uma definição padronizada, versionável e automatizável da infraestrutura Azure mínima de uma aplicação EmpresaX. Como consequência:

- a preparação de novos ambientes depende de conhecimento tácito e intervenção manual;
- os ambientes podem divergir em recursos, nomes e configurações;
- a passagem de DEV para QA e PRD não possui um artefato único e reproduzível;
- os templates de aplicação não têm um contrato estável para consumir nomes, identificadores e endpoints da infraestrutura;
- novas aplicações ficam sujeitas a omissões de segurança, identidade, configuração, persistência, mensageria, armazenamento, exposição e integração com recursos compartilhados.

## 3. Objetivos

- Disponibilizar um template de infraestrutura como código que prepare todos os recursos mínimos de uma aplicação EmpresaX para um ambiente informado por parâmetro.
- Garantir que 100% do provisionamento previsto possa ser executado sem perguntas, confirmações ou preenchimentos interativos durante a execução.
- Garantir idempotência: uma nova execução com os mesmos parâmetros e sem alteração da especificação não deve criar recursos duplicados nem propor mudanças indevidas.
- Padronizar nomes e referências para DEV, QA e PRD, respeitando as convenções corporativas e as restrições de nomes dos serviços Azure.
- Permitir que o desenvolvedor valide e aplique a infraestrutura em DEV e entregue exatamente a mesma definição, com parâmetros próprios do ambiente, para aplicação controlada pela equipe de infraestrutura em QA e PRD.
- Produzir saídas consumíveis pelos templates de backend e frontend, reduzindo a necessidade de redigitação ou descoberta manual de dados da infraestrutura.

## 4. Usuários principais

### 4.1 Desenvolvedor de aplicação

Precisa criar e validar autonomamente a infraestrutura de DEV, compreender os parâmetros necessários e obter as referências que serão usadas pela aplicação.

### 4.2 Equipe de infraestrutura/DevOps

Precisa receber uma definição declarativa, previsível e revisável para aplicar em QA e PRD, preservando controles de acesso e governança desses ambientes.

## 5. Histórias de usuário

- Como desenvolvedor, quero informar a aplicação e o ambiente DEV para criar toda a infraestrutura mínima sem executar etapas manuais no portal Azure.
- Como desenvolvedor, quero executar novamente o template com os mesmos parâmetros para confirmar que o ambiente permanece estável e sem recursos duplicados.
- Como desenvolvedor, quero receber nomes, identificadores e endpoints relevantes após o provisionamento para configurar a aplicação criada pelos templates de backend e frontend.
- Como membro da equipe de infraestrutura, quero receber os mesmos arquivos validados em DEV com parâmetros de QA ou PRD para revisar e aplicar a infraestrutura sem reinterpretar a necessidade da aplicação.
- Como membro da equipe de infraestrutura, quero que recursos corporativos compartilhados sejam apenas referenciados para impedir recriação, substituição ou remoção acidental.
- Como responsável pela aplicação, quero que identidade, segredos, configuração, execução, mensageria, armazenamento, banco, namespace, DNS e TLS estejam preparados de maneira consistente em cada ambiente.

## 6. Fluxos principais

### 6.1 Criação em DEV

1. O desenvolvedor informa os parâmetros obrigatórios, incluindo aplicação e ambiente `dev`.
2. A solução valida parâmetros, convenções de nomes, limites dos serviços e existência dos recursos compartilhados.
3. A solução cria ou atualiza declarativamente os recursos exclusivos da aplicação no `rg-dev` e configura as integrações necessárias.
4. A solução referencia `aks-dev` e `pgsql-dev`, sem tentar criá-los ou assumir sua propriedade.
5. A solução apresenta saídas estruturadas para consumo pela aplicação e para diagnóstico.
6. Uma repetição sem mudanças termina sem duplicações e sem alterações materiais.

### 6.2 Promoção para QA e PRD

1. O desenvolvedor entrega os arquivos versionados e o conjunto de parâmetros do ambiente à equipe de infraestrutura.
2. A equipe de infraestrutura revisa a mudança e executa a mesma definição para `qa` ou `prd`.
3. A solução seleciona o resource group e os recursos compartilhados correspondentes ao ambiente.
4. A solução cria ou atualiza os recursos exclusivos e fornece as mesmas categorias de saída produzidas em DEV.

### 6.3 Atualização de uma aplicação existente

1. Um parâmetro ou requisito do template é alterado de forma versionada.
2. A solução calcula e aplica somente as diferenças necessárias.
3. Recursos compartilhados ou não pertencentes à aplicação não são modificados ou removidos.

## 7. Requisitos funcionais

### RF-01 — Seleção de ambiente

O template deve receber um único ambiente por execução, limitado a `dev`, `qa` ou `prd`, e mapear respectivamente:

| Ambiente | Resource group | AKS compartilhado | PostgreSQL Flex compartilhado |
|---|---|---|---|
| `dev` | `rg-dev` | `aks-dev` | `pgsql-dev` |
| `qa` | `rg-qa` | `aks-qa` | `pgsql-qa` |
| `prd` | `rg-prd` | `aks-prd` | `pgsql-prd` |

Valores ausentes ou diferentes dos permitidos devem ser rejeitados antes de qualquer alteração.

### RF-02 — Parâmetros da aplicação

O template deve aceitar os dados mínimos para formar nomes técnicos e o nome de exibição da aplicação. Deve validar e normalizar esses dados conforme as regras de cada recurso, sinalizando entradas que não possam gerar nomes válidos ou inequívocos.

### RF-03 — App Registration

Deve existir uma App Registration exclusiva da aplicação e do ambiente, com nome de exibição no padrão `EmpresaX <Nome Aplicação> <Ambiente>`.

### RF-04 — Managed Identity

Deve existir uma managed identity no padrão `mnid-<aplicacao>-<ambiente>`.

### RF-05 — App Configuration

Deve existir um App Configuration no padrão `apcfg-<aplicacao>-<ambiente>`.

### RF-06 — Key Vault

Deve existir um Key Vault no padrão `kv-<aplicacao>-<ambiente>`.

### RF-07 — Azure Function

O escopo inicial deve criar uma Azure Function para o escopo geral, no padrão `fn-<aplicacao>-geral-<ambiente>`. A solução deve deixar uma evolução futura para múltiplos escopos possível, mas essa generalização não faz parte da primeira entrega.

### RF-08 — Event Hubs

O escopo inicial deve criar um Event Hubs Namespace para o escopo geral, no padrão `evhns-<aplicacao>-geral-<ambiente>`. Entidades internas adicionais somente serão criadas se forem definidas posteriormente como parte dos requisitos mínimos da aplicação.

### RF-09 — Namespace no AKS compartilhado

Deve existir o namespace `ns-<aplicacao>` no cluster AKS correspondente ao ambiente. O cluster deve ser referenciado como recurso compartilhado e não pode ser criado, substituído ou removido pelo template.

### RF-10 — Banco no PostgreSQL Flex compartilhado

Deve existir uma base com o nome técnico da aplicação dentro de `pgsql-<ambiente>`. O servidor deve ser referenciado como recurso compartilhado e não pode ser criado, substituído ou removido pelo template.

### RF-11 — Storages obrigatórios

Devem existir dois storages exclusivos por aplicação e ambiente:

- escopo geral, no padrão lógico `stg<aplicacao>geral<ambiente>`, destinado a logs e arquivos;
- escopo `func`, no padrão lógico `stg<aplicacao>func<ambiente>`, destinado ao deploy das Functions e ao controle de checkpoint do Event Hub.

Como nomes de Storage Account possuem restrições e unicidade global, o template deve aplicar uma regra determinística de normalização e desambiguação, documentando o nome efetivamente criado.

### RF-12 — Subdomínio, DNS e TLS

O namespace da aplicação deve ficar acessível pelo endereço `https://<aplicacao>-<ambiente>.empresax.com.br`, seguindo o comportamento exemplificado por `https://tickets-checklists-dev.empresax.com.br` e `https://outro-sistema-qa.empresax.com.br`. A infraestrutura deve deixar DNS, roteamento/ingresso e TLS prontos, usando os componentes corporativos existentes quando aplicável.

### RF-13 — Permissões e vínculos

A infraestrutura deve suportar a configuração das permissões e vínculos necessários entre App Registration, managed identity, App Configuration, Key Vault, Function, Event Hubs, storages, AKS e PostgreSQL. A matriz exata de roles será fornecida posteriormente e é uma dependência para concluir a especificação técnica e a implementação.

### RF-14 — Referências a recursos compartilhados

Resource groups, clusters AKS e servidores PostgreSQL Flex existentes devem ser tratados como dependências externas. A solução deve verificar sua existência e falhar com mensagem acionável quando a referência esperada não estiver disponível.

### RF-15 — Saídas do provisionamento

A execução deve fornecer, em formato estruturado e sem expor segredos, pelo menos nomes, identificadores e endpoints necessários para integrar a aplicação aos recursos criados ou referenciados. A relação final de saídas deverá ser definida na TechSpec a partir dos contratos dos templates de backend e frontend.

### RF-16 — Documentação de uso

O template deve incluir documentação para:

- pré-requisitos e permissões de execução;
- parâmetros obrigatórios e opcionais;
- criação ou atualização em DEV;
- preparação dos parâmetros e entrega para QA e PRD;
- interpretação das saídas;
- validação de idempotência;
- diagnóstico e recuperação de falhas sem recorrer ao portal para completar manualmente o provisionamento.

### RF-17 — Compatibilidade com templates existentes

A entrega deve avaliar as estruturas atuais dos templates de backend e frontend e definir um ponto de integração coerente com suas convenções de diretórios, configuração e documentação. O template de infraestrutura não deve acoplar-se a uma linguagem ou framework específico.

## 8. Requisitos não funcionais

### RNF-01 — Idempotência

Duas execuções consecutivas com a mesma versão e os mesmos parâmetros, sem mudanças externas, devem resultar no mesmo estado desejado e não criar recursos duplicados. A segunda avaliação deve indicar ausência de alterações materiais.

### RNF-02 — Execução não interativa

Todos os dados necessários devem ser fornecidos por parâmetros, arquivos de parâmetros, identidade da execução ou configuração previamente documentada. A execução não pode depender de prompts, cliques no portal ou confirmações humanas durante o provisionamento.

### RNF-03 — Segurança

- Segredos e credenciais não podem ser versionados, impressos como saída ou armazenados em texto simples.
- O acesso deve seguir menor privilégio conforme a matriz de roles a ser fornecida.
- Operações privilegiadas, especialmente criação de App Registration e alterações em recursos compartilhados, devem ter pré-requisitos explícitos.
- A solução deve separar claramente permissões disponíveis ao desenvolvedor em DEV das permissões reservadas à equipe de infraestrutura em QA e PRD.

### RNF-04 — Previsibilidade

Nomes derivados dos mesmos parâmetros devem ser determinísticos. Normalização, truncamento, tratamento de caracteres inválidos e resolução de unicidade devem ser documentados e testáveis.

### RNF-05 — Proteção de recursos compartilhados

O ciclo de vida do template não pode incluir exclusão ou recriação dos resource groups, clusters AKS e servidores PostgreSQL Flex corporativos.

### RNF-06 — Observabilidade da execução

Falhas devem identificar o recurso e a causa provável sem revelar informação sensível. O resultado deve permitir distinguir validação, criação, atualização, ausência de mudança e falha.

### RNF-07 — Manutenibilidade

A solução deve permitir evolução versionada, composição modular e inclusão futura de múltiplos escopos sem exigir duplicação integral da definição base.

### RNF-08 — Portabilidade operacional

A mesma definição lógica deve ser utilizável nos três ambientes apenas pela troca dos parâmetros autorizados, sem ramificações manuais do código por ambiente.

## 9. Critérios de aceite

- Uma aplicação de teste pode ter a infraestrutura de DEV preparada por uma execução não interativa com parâmetros documentados.
- A execução cria ou configura App Registration, managed identity, App Configuration, Key Vault, uma Function geral, um Event Hubs Namespace geral, namespace no AKS, banco no PostgreSQL Flex, dois storages, DNS/ingresso e TLS conforme os padrões definidos.
- `rg-dev`, `rg-qa`, `rg-prd`, `aks-dev`, `aks-qa`, `aks-prd`, `pgsql-dev`, `pgsql-qa` e `pgsql-prd` são apenas referenciados e permanecem fora do ciclo de vida da solução.
- Uma segunda execução sem mudanças não cria duplicações e reporta ausência de alterações materiais.
- Entradas inválidas ou dependências compartilhadas ausentes são detectadas antes de alterações parciais sempre que a plataforma permitir validação prévia.
- O template não solicita interação humana durante a execução.
- Nenhum segredo ou credencial é incluído no repositório ou nas saídas.
- As saídas permitem configurar os pontos de integração esperados pelos templates de backend e frontend.
- Os mesmos arquivos podem ser entregues à equipe de infraestrutura e avaliados com parâmetros de QA e PRD sem alteração manual da definição.
- A documentação permite que um novo usuário execute o fluxo de DEV e prepare a entrega para ambientes controlados.
- Testes automatizados ou validações equivalentes cobrem a formação dos nomes, o mapeamento de ambientes, a proteção de recursos compartilhados e a idempotência.

## 10. Métricas de sucesso

- 100% dos recursos mínimos definidos neste PRD são representados e gerenciados declarativamente, exceto os recursos explicitamente classificados como compartilhados.
- 100% das execuções padrão podem ocorrer sem prompts ou ações no portal Azure.
- 0 recursos duplicados após reaplicação com entradas idênticas.
- 0 mudanças propostas sobre o ciclo de vida dos recursos compartilhados em uma reaplicação ou atualização normal.
- 100% dos três ambientes utilizam a mesma definição lógica, variando somente por parâmetros autorizados.
- 100% dos dados de integração definidos como saída são produzidos sem exposição de segredos.

Não há meta de duração do provisionamento nesta fase; velocidade não é um indicador de sucesso do produto.

## 11. Fora do escopo

- Implementação de pipelines de CI/CD para provisionamento, promoção ou deploy de aplicações.
- Suporte inicial a múltiplos escopos de negócio por aplicação.
- Criação ou gestão do ciclo de vida dos resource groups, clusters AKS e servidores PostgreSQL Flex compartilhados.
- Definição unilateral da matriz corporativa de roles e permissões antes de seu fornecimento pelo responsável.
- Desenvolvimento ou alteração do código dos templates de backend e frontend além dos pontos de integração estritamente necessários ao template de infraestrutura.
- Operação cotidiana, monitoramento funcional ou sustentação das aplicações provisionadas.
- Interface gráfica de provisionamento.
- Migração automática de infraestrutura criada anteriormente fora do template.

## 12. Dependências

- Existência e acesso aos resource groups `rg-dev`, `rg-qa` e `rg-prd`.
- Existência e acesso de leitura/integração aos clusters `aks-dev`, `aks-qa` e `aks-prd`.
- Existência e acesso administrativo adequado aos servidores `pgsql-dev`, `pgsql-qa` e `pgsql-prd` para criação de bases e, se necessário, identidades/usuários.
- Disponibilidade da zona DNS de `empresax.com.br`, do mecanismo de ingresso do AKS e da emissão/associação de certificados TLS.
- Permissão de diretório Microsoft Entra ID para criar ou reconciliar App Registrations.
- Relação de roles e vínculos necessários entre os recursos, a ser fornecida antes da conclusão da TechSpec.
- Definição dos contratos de configuração e das saídas consumidas pelos templates atuais de backend e frontend.
- Assinatura, tenant, região, SKUs, políticas Azure, redes privadas e demais padrões corporativos, a serem confirmados na TechSpec.

## 13. Riscos e mitigação

| Risco | Impacto | Mitigação esperada |
|---|---|---|
| Restrições e unicidade global de nomes, especialmente Storage Account e Key Vault | Nomes inválidos ou colisões | Definir normalização e sufixo determinístico na TechSpec; validar antes de aplicar |
| App Registration pertence ao escopo do tenant, não somente ao resource group | Permissões insuficientes ou ciclo de vida inconsistente | Separar claramente escopos de implantação e documentar as permissões exigidas |
| Matriz de roles ainda não fornecida | Ambiente criado, porém sem integrações utilizáveis | Tratar a matriz como requisito de entrada obrigatório antes da implementação final |
| Alterações acidentais em AKS ou PostgreSQL compartilhados | Impacto em diversas aplicações | Referenciar recursos existentes e excluir seu ciclo de vida do template |
| Diferenças de permissão entre DEV, QA e PRD | Funcionamento em DEV sem possibilidade de promoção | Validar o fluxo de entrega e revisão com a equipe de infraestrutura desde a TechSpec |
| Ambiguidade entre o nome do namespace e o host público | DNS incorreto | Formalizar na TechSpec que o namespace é `ns-<aplicacao>` e o host público é `<aplicacao>-<ambiente>.empresax.com.br` |
| Limites de tamanho após composição de aplicação, escopo e ambiente | Falha na criação de recursos | Estabelecer limites de entrada, abreviação e hash determinístico |
| Operações parciais quando uma dependência externa estiver indisponível | Ambiente incompleto | Executar validações prévias e garantir reaplicação segura |

## 14. Diretrizes de experiência e acessibilidade

Não haverá interface gráfica. A experiência deve ser orientada a CLI e arquivos versionados, com:

- nomes de parâmetros consistentes e mensagens claras;
- validação antecipada e erros acionáveis;
- exemplos completos para DEV, QA e PRD;
- saídas estruturadas, legíveis por humanos e ferramentas;
- documentação em texto acessível, sem depender exclusivamente de cor, ícones ou formatação visual para transmitir estado.

## 15. Premissas e questões em aberto

### Premissas registradas

- Cada execução trata somente um ambiente.
- A primeira versão usa apenas o escopo `geral` para Function e Event Hubs Namespace.
- Todo aplicativo nasce com os storages lógicos `geral` e `func`.
- O prefixo correto do Key Vault é `kv-`.
- Recursos compartilhados já existem e não pertencem ao ciclo de vida deste template.
- Desenvolvedores aplicam em DEV; a equipe de infraestrutura aplica em QA e PRD.
- Pipelines serão tratados em outra especificação.

### Questões para a TechSpec

- A implementação adotará Bicep, Terraform ou outra composição aprovada?
- Quais módulos e convenções dos templates de backend e frontend devem ser reutilizados?
- Quais roles, escopos e vínculos devem ser aplicados entre cada identidade e recurso?
- Quais SKUs, região, configurações de rede, private endpoints, políticas de retenção, redundância, backup e proteção contra exclusão são obrigatórios por ambiente?
- Quais entidades internas do Event Hubs, consumidores, containers, tabelas ou compartilhamentos precisam existir na configuração inicial?
- Qual mecanismo corporativo fornece ingresso, DNS e certificados para o namespace do AKS?
- Como autenticar no PostgreSQL e quais usuários, schemas, extensões e privilégios mínimos devem acompanhar a base?
- Quais saídas e arquivos de configuração os templates de backend e frontend esperam consumir?
- Qual regra determinística resolverá restrições e colisões de nomes globais?
