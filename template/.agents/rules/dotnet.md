# Regras para .NET

Estas regras se aplicam à API, aos workers e aos projetos de infraestrutura do backend.

## Código e dependências

- Mantenha Controllers e hosts finos; regras de negócio pertencem a `Backend.App` e `Backend.Domain`.
- Passe `CancellationToken` por toda operação assíncrona que o receba.
- Use `IOptions<T>` ou `IOptionsMonitor<T>` para configuração tipada; valide opções na inicialização.
- Não acople `Backend.App` ou `Backend.Domain` a ASP.NET Core, EF Core, Hangfire, Redis, Azure SDKs ou a fornecedores externos.
- Implemente integrações de saída apenas em `Backend.Infra.Gate.<Fornecedor>` e mantenha os DTOs externos dentro do respectivo projeto.

## Operação

- Configure timeouts, retries e circuit breakers de forma explícita com Polly. Não acumule retries independentes sem definir o limite total de tentativas.
- Jobs, consumidores de eventos e processos de polling devem ser idempotentes e registrar correlação, tenant e resultado do processamento.
- Workers devem respeitar cancelamento e encerrar graciosamente quando o host receber sinal de desligamento.
- Nunca exponha segredos ou tokens em logs, exceptions ou arquivos versionados.

## Autenticação e autorização

- A API usa Microsoft Entra ID para autenticação OAuth 2.0/OpenID Connect.
- A autorização deve ser declarada por policies e aplicada nos Controllers ou endpoints administrativos.
- Não transporte o token de acesso do usuário como payload de job ou evento. Transporte apenas a identidade e o contexto mínimo necessário, quando autorizado.
