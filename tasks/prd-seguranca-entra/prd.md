# Documento de Requisitos do Produto (PRD)

## Visão geral

Este produto de exemplo demonstra uma evolução do modelo de identidade, autorização e auditoria usado pelas aplicações da EmpresaX. Atualmente, cerca de 20 aplicações usam usuários do Microsoft Entra ID, App Registrations com roles de perfil e integração REST autenticada por client secret compartilhado. Esse modelo expõe e-mails nos registros de auditoria e dificulta identificar, autorizar e revogar com segurança as integrações entre aplicações e com fornecedores.

A proposta cria um padrão único para três atores: usuário humano interno, aplicação da EmpresaX e aplicação de fornecedor. O padrão preserva o modelo de permissões por funcionalidade existente, reduz a exposição de dados pessoais nos registros e assegura que cada operação possa ser atribuída a uma identidade técnica ou humana autorizada.

## Objetivos

- Demonstrar, em uma aplicação exemplo, o controle de acesso e auditoria para ao menos um usuário interno, uma integração entre aplicações da EmpresaX e uma integração REST de fornecedor.
- Garantir que 100% das operações de criação e alteração da aplicação exemplo registrem o tipo de ator e um identificador rastreável sem gravar e-mail ou nome como identificador de auditoria.
- Garantir que uma identidade de aplicação sem permissão explícita seja recusada em 100% dos endpoints protegidos do exemplo.
- Permitir a revogação independente do acesso de uma aplicação interna ou de um fornecedor, sem afetar os demais consumidores.
- Eliminar, no cenário demonstrado, o compartilhamento de client secrets de longa duração com fornecedores.

## Histórias de usuário

- US1: Como usuário interno da EmpresaX, quero acessar somente as funcionalidades compatíveis com meu perfil e minhas liberações organizacionais para executar minhas atividades com segurança.
- US2: Como auditor ou administrador autorizado, quero identificar posteriormente o usuário responsável por uma alteração a partir de um identificador de auditoria para investigar ocorrências sem expor seu e-mail na base da aplicação.
- US3: Como aplicação da EmpresaX, quero chamar uma API de outra aplicação usando minha própria identidade e permissões para que as alterações sejam auditáveis como ações sistêmicas, e não como ações sem autor.
- US4: Como administrador de integração, quero conceder a um fornecedor REST somente as operações contratadas e identificar inequivocamente qual fornecedor realizou cada chamada.
- US5: Como administrador de segurança, quero revogar ou reduzir o acesso de uma integração específica sem interromper integrações autorizadas de outros fornecedores ou aplicações.

## Principais funcionalidades

### Identidade de auditoria pseudonimizada

A aplicação exemplo deve registrar um identificador estável do ator, seu tipo e a origem da identidade, em vez de usar e-mail como `CreatorUsername` ou equivalente. O identificador deve permitir uma consulta posterior, por pessoa autorizada, ao usuário correspondente no Microsoft Entra ID.

- RF1: Registrar, em cada criação e alteração, o identificador de auditoria, o tipo de ator (usuário interno, aplicação interna ou fornecedor) e a data/hora.
- RF2: Não usar e-mail ou nome como identificador persistido de criação ou alteração.
- RF3: Permitir a rastreabilidade do identificador de auditoria até a identidade correspondente no Microsoft Entra ID mediante autorização administrativa.

### Autorização de usuários internos

A aplicação exemplo deve preservar a capacidade atual de conceder perfis de acesso e liberações de escopo, como filiais, e relacioná-los às funcionalidades protegidas pela aplicação.

- RF4: Autorizar usuários internos por perfis e escopos atribuídos no Microsoft Entra ID.
- RF5: Aplicar as permissões recebidas à funcionalidade e ao escopo correspondente da aplicação.
- RF6: Negar acesso a funcionalidades ou escopos que não tenham permissão atribuída.

### Identidade e autorização entre aplicações da EmpresaX

Integrações REST sem usuário humano devem usar uma identidade de aplicação própria, com permissões explícitas para a API consumida, e devem gerar auditoria atribuível a essa aplicação.

- RF7: Distinguir operações executadas por aplicação das executadas por usuário humano.
- RF8: Autorizar uma aplicação interna apenas para as operações REST explicitamente concedidas.
- RF9: Registrar a identidade da aplicação responsável em cada alteração originada por integração.

### Acesso REST de fornecedores

Cada fornecedor, ou cada sistema de fornecedor que exija ciclo de vida e revogação independentes, deve possuir um App Registration próprio no Microsoft Entra ID. O fornecedor deve receber o `ClientId` desse App Registration como identificador público de sua integração. As permissões de API determinam o que essa identidade pode fazer; a associação administrativa entre o `ClientId`, o App Registration e o fornecedor determina quem realizou cada chamada.

- RF10: Criar um App Registration específico para cada fornecedor, fornecer seu `ClientId` ao fornecedor e manter uma associação inequívoca entre ambos.
- RF11: Conceder a cada fornecedor somente as permissões REST necessárias ao contrato de integração.
- RF12: Identificar, nos registros de auditoria, o fornecedor e sua identidade de aplicação para cada operação realizada.
- RF13: Permitir revogar ou alterar as permissões de um fornecedor sem afetar outros fornecedores.
- RF14: Não compartilhar uma credencial secreta de longa duração com fornecedores; a forma de autenticação de maior segurança será definida na TechSpec.

## Critérios de aceitação

- CA-01 (US1, RF4-RF6): Dado um usuário interno com perfil e escopo atribuídos, quando acessar uma funcionalidade autorizada, então a aplicação permite o acesso somente dentro daquele escopo.
- CA-02 (US1, RF6): Dado um usuário interno sem perfil ou escopo necessário, quando tentar acessar a funcionalidade protegida, então a aplicação recusa a operação.
- CA-03 (US2, RF1-RF3): Dada uma alteração realizada por usuário interno, quando o registro for consultado, então ele contém um identificador rastreável e não contém e-mail ou nome como identificador de auditoria.
- CA-04 (US2, RF3): Dado um identificador de auditoria de usuário, quando um administrador autorizado o consulta, então consegue localizar a identidade correspondente no Microsoft Entra ID.
- CA-05 (US3, RF7-RF9): Dada uma chamada REST autorizada de uma aplicação interna, quando ela altera um registro, então a alteração fica identificada como ação da aplicação responsável.
- CA-06 (US3, RF8): Dada uma aplicação interna sem permissão para uma operação, quando ela chama o endpoint protegido, então a API recusa a solicitação.
- CA-07 (US4, RF10): Dado o onboarding de um fornecedor, quando sua integração é provisionada, então ele recebe o `ClientId` de um App Registration específico e associado exclusivamente à sua identidade de integração.
- CA-08 (US4, RF10-RF12): Dada uma chamada REST autorizada de um fornecedor, quando ela altera um registro, então a aplicação registra a identidade da integração e o fornecedor associado.
- CA-09 (US4, RF11): Dada uma identidade de fornecedor com permissão limitada, quando ela chama uma operação fora de seu contrato, então a API recusa a solicitação.
- CA-10 (US5, RF13): Dada a revogação de uma identidade de fornecedor, quando ela tenta acessar a API, então seu acesso é recusado e as integrações dos demais fornecedores permanecem operantes.
- CA-11 (US5, RF14): Dada uma integração de fornecedor do cenário exemplo, quando suas credenciais são verificadas, então não há client secret de longa duração compartilhado com o fornecedor.

## Experiência do usuário

Os usuários internos continuam usando o login corporativo existente; a evolução não deve introduzir uma nova jornada de autenticação para eles. A aplicação exemplo deve apresentar mensagens claras de acesso negado sem revelar detalhes de permissões, identidades técnicas ou configuração de segurança.

Administradores de segurança e integração precisam de uma visão administrativa — mesmo que inicialmente documentada ou demonstrada por configuração controlada — para relacionar identidades técnicas a aplicações ou fornecedores, revisar permissões e consultar a auditoria. As informações de auditoria devem ser compreensíveis para investigação, mas dados pessoais só podem ser revelados a pessoas autorizadas.

## Restrições técnicas de alto nível

- Microsoft Entra ID é o provedor de identidade obrigatório para usuários, aplicações internas e identidades de fornecedores.
- As aplicações existentes possuem App Registrations e roles que representam perfis e liberações; a proposta deve preservar a continuidade desse modelo enquanto define sua evolução.
- A comunicação entre aplicações e com fornecedores ocorre por endpoints REST protegidos.
- A autorização de aplicações deve seguir o princípio do menor privilégio e exigir permissão explícita por operação ou conjunto de operações.
- Segredos compartilhados de longa duração com fornecedores não são aceitáveis no cenário alvo; a TechSpec avaliará credenciais baseadas em certificado e federação de identidade, conforme a capacidade do fornecedor.
- O identificador de auditoria, mesmo sem e-mail ou nome, deve ser tratado como dado pessoal pseudonimizado e protegido conforme a LGPD e as políticas internas.
- O produto exemplo deve manter registros suficientes para auditoria, investigação e revogação de acessos; prazos de retenção e requisitos de monitoramento serão definidos com Segurança e Compliance.

## Fora do escopo

- Migração imediata das aproximadamente 20 aplicações da EmpresaX; o escopo é uma aplicação exemplo e uma proposta reutilizável de evolução.
- Acesso de pessoas de fornecedores por portal, colaboração B2B ou contas de usuário externas.
- Definição do protocolo, bibliotecas, configuração de tokens, banco de dados ou código de autorização; esses detalhes pertencem à TechSpec.
- Substituição integral, nesta etapa, das funcionalidades e tabelas de autorização já existentes em cada aplicação.
- Implementação de uma central de governança ou interface administrativa completa para todas as aplicações.
