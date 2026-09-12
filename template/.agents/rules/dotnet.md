# Regras para .NET

Estas regras se aplicam à API, aos workers e aos projetos de infraestrutura do backend.

## Código e dependências

- Mantenha Controllers e hosts finos; regras de negócio pertencem a `Backend.App` e `Backend.Domain`.
- Passe `CancellationToken` por toda operação assíncrona que o receba.
- Use `IOptions<T>` ou `IOptionsMonitor<T>` para configuração tipada; valide opções na inicialização.
- Não acople `Backend.App` ou `Backend.Domain` a ASP.NET Core, EF Core, Hangfire, Redis, Azure SDKs ou a fornecedores externos.
- Implemente integrações de saída apenas em `Backend.Infra.Gate.<Fornecedor>` e mantenha os DTOs externos dentro do respectivo projeto.

## Modelagem do domínio

- Declare cada classe, record, interface e enum de domínio em seu próprio arquivo, com o mesmo nome do tipo.
- Mantenha enums próximos ao agregado ou funcionalidade a que pertencem; não crie uma pasta global `Domain.Enums`.
- Atribua explicitamente o valor numérico de todos os membros de enums. Nunca dependa da ordem de declaração, não renumere valores persistidos e não reutilize valores removidos.
- Anote todo membro de enum com `DescriptionAttribute` e uma descrição de negócio clara.
- Só declare um membro de valor zero quando zero representar um estado válido. Caso contrário, represente ausência com enum nullable e valide entradas externas.
- Em propriedades de enum persistidas pelo EF Core, use `HasEnumComment()` para armazenar como inteiro e gerar no schema o comentário `valor = membro — descrição`.

Organize os membros de uma classe nesta ordem:

1. constantes e campos estáticos;
2. campos de instância;
3. propriedades;
4. construtores;
5. métodos públicos;
6. métodos protegidos e privados.

## Contratos

- Contratos HTTP pertencem ao projeto da API; eventos, jobs e mensagens compartilhados entre processos pertencem a `Backend.Contracts`.
- Quando um DTO possuir um tipo auxiliar exclusivo, declare o auxiliar como tipo público aninhado no DTO principal. Não use um tipo `file` na assinatura pública, pois tipos locais ao arquivo não podem ser expostos por membros públicos.
- Contratos compartilhados devem conter apenas dados serializáveis, sem lógica de negócio, e devem ser versionados quando cruzarem fronteiras de processo.

## Result Pattern

- Use `IResponse<T>` para falhas esperadas de negócio ou aplicação e preserve exceções para falhas inesperadas, defeitos de programação e indisponibilidades que não possam ser tratadas no nível atual.
- `IResponse` expõe `Result`, `Success`, `HttpStatusCode`, `RegrasNegocio`, `Erros`, `Warnings`, `Informations` e `Debugs`; as coleções são somente leitura externamente e contêm mensagens estruturadas.
- `Append` retorna o resultado tipado, agrega todas as mensagens e combina sucesso com `Success = Success && response.Success`.
- Ao agregar falhas, preserve o primeiro `HttpStatusCode` não nulo identificado. Nunca substitua um status já registrado.
- Uma falha não pode informar status HTTP abaixo de 400. Quando uma falha não informar status, o filtro da API usa 400.
- O filtro global da API serializa apenas `Result` em respostas de sucesso e mantém o envelope completo em respostas com falha.
- Não exponha mensagens de `Debugs` em ambientes de produção.

## Operação

- Configure timeouts, retries e circuit breakers de forma explícita com Polly. Não acumule retries independentes sem definir o limite total de tentativas.
- Jobs, consumidores de eventos e processos de polling devem ser idempotentes e registrar correlação, tenant e resultado do processamento.
- Workers devem respeitar cancelamento e encerrar graciosamente quando o host receber sinal de desligamento.
- Nunca exponha segredos ou tokens em logs, exceptions ou arquivos versionados.

## Autenticação e autorização

- A API usa Microsoft Entra ID para autenticação OAuth 2.0/OpenID Connect.
- A autorização deve ser declarada por policies e aplicada nos Controllers ou endpoints administrativos.
- Não transporte o token de acesso do usuário como payload de job ou evento. Transporte apenas a identidade e o contexto mínimo necessário, quando autorizado.
