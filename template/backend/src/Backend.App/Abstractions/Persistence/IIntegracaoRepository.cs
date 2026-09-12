using Backend.Domain.Integracoes;

namespace Backend.App.Abstractions.Persistence;

public interface IIntegracaoRepository
{
    public Task<IReadOnlyCollection<Integracao>> BuscarEIniciarPendentesAsync(int limite, CancellationToken cancellationToken);
    public Task<Integracao?> ObterAsync(Guid id, CancellationToken cancellationToken);
    public Task SalvarAsync(Integracao integracao, CancellationToken cancellationToken);
}
