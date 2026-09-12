using Backend.App.Abstractions.Persistence;
using Backend.Domain.Integracoes;
using Backend.Domain.Tickets;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infra.Postgres.Persistence;

internal sealed class TicketRepository(BackendDbContext dbContext) : ITicketRepository
{
    public Task<Guid?> ObterIdPorEventoAsync(string eventId, CancellationToken cancellationToken) =>
        dbContext.EventosRecebidos.Where(x => x.EventId == eventId).Select(x => (Guid?)x.TicketId).SingleOrDefaultAsync(cancellationToken);

    public Task<Ticket?> ObterAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Tickets.Include(x => x.Checklist).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task SalvarRecebidoAsync(Ticket ticket, string eventId, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        dbContext.Tickets.Add(ticket);
        dbContext.EventosRecebidos.Add(new EventoRecebido(eventId, ticket.Id));
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task SalvarChecklistEIntegracaoAsync(Ticket ticket, Integracao integracao, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        dbContext.Integracoes.Add(integracao);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
