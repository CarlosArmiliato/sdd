# Backend empresarial

Template .NET 10 de mini-monólito modular com API por Controllers e hosts separados para Hangfire, polling e Azure Event Hubs.

## Executar

Configure as variáveis descritas em `.env.example` por variáveis de ambiente, User Secrets ou configuração do AKS. Depois:

```powershell
dotnet restore Backend.slnx
dotnet build Backend.slnx --no-restore
dotnet test Backend.slnx --no-build --no-restore
dotnet run --project src/Backend.Api
```

Em desenvolvimento, a referência interativa dos Controllers fica em `/scalar/v1` e o documento OpenAPI em `/openapi/v1.json`.

## Fluxo de exemplo

1. Fazenda, Ano Agrícola, Safra e Cultura possuem CRUD REST e sincronização diária a partir do SAP.
2. `Backend.Worker.EventHub` consome `TicketRecebidoV1`, grava o ticket e confirma o checkpoint somente depois da transação local.
3. A resposta dos itens `Item1`, `Item2` e `Item3` grava o checklist e uma linha em `Integracoes` na mesma transação.
4. `Backend.Worker.Polling` reserva registros com `FOR UPDATE SKIP LOCKED`, resolve o handler pelo tipo e publica o resultado para o Event Hub.
5. Falhas usam backoff exponencial, limite de tentativas e recuperação de leases expirados.

Consulte `ARCHITECTURE.md` para as regras de dependência e responsabilidades dos projetos.
