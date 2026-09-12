using System.Text.Json;
using Backend.App.Abstractions.Integrations;
using Backend.Contracts.Events;
using Backend.Domain.Integracoes;

namespace Backend.Infra.Gate.EmpresaA;

public sealed class ResultadoChecklistEmpresaAHandler(IIntegrationEventPublisher publisher) : IIntegracaoHandler
{
    public const string IntegrationType = "ResultadoChecklistEmpresaA.v1";
    public string Tipo => IntegrationType;

    public async Task HandleAsync(Integracao integracao, CancellationToken cancellationToken)
    {
        ResultadoChecklistEmpresaAV1 message = JsonSerializer.Deserialize<ResultadoChecklistEmpresaAV1>(integracao.Payload)
            ?? throw new InvalidOperationException("Payload da integração Empresa A é inválido.");
        await publisher.PublishAsync(integracao.Id.ToString(), message, cancellationToken);
    }
}
