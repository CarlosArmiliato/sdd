using Backend.Domain.Integracoes;

namespace Backend.App.Abstractions.Integrations;

public interface IIntegracaoHandler
{
    public string Tipo { get; }
    public Task HandleAsync(Integracao integracao, CancellationToken cancellationToken);
}
