using Backend.Domain.Integracoes;
using Backend.Domain.Tickets;

namespace Backend.App.Abstractions.Persistence;

public interface ITicketRepository
{
    public Task<Guid?> ObterIdPorEventoAsync(string eventId, CancellationToken cancellationToken);
    public Task<Ticket?> ObterAsync(Guid id, CancellationToken cancellationToken);
    public Task SalvarRecebidoAsync(Ticket ticket, string eventId, CancellationToken cancellationToken);
    public Task SalvarChecklistEIntegracaoAsync(
        Ticket ticket,
        Integracao integracao,
        CancellationToken cancellationToken);
}
