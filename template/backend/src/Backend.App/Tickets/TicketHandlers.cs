using System.Text.Json;
using Backend.App.Abstractions;
using Backend.App.Abstractions.Persistence;
using Backend.Contracts.Events;
using Backend.Domain.Integracoes;
using Backend.Domain.Tickets;
using Cortex.Mediator.Commands;
using Cortex.Mediator.Queries;

namespace Backend.App.Tickets;

public sealed class ReceberTicketHandler(ITicketRepository repository)
    : ICommandHandler<ReceberTicketCommand, Guid>
{
    public async Task<Guid> Handle(ReceberTicketCommand command, CancellationToken cancellationToken)
    {
        TicketRecebidoV1 evento = command.Evento;
        Guid? ticketExistente = await repository.ObterIdPorEventoAsync(evento.EventId, cancellationToken);
        if (ticketExistente.HasValue)
        {
            return ticketExistente.Value;
        }

        Ticket ticket = new(evento.NumeroTicket, evento.CorrelationId);
        await repository.SalvarRecebidoAsync(ticket, evento.EventId, cancellationToken);
        return ticket.Id;
    }
}

public sealed class ObterTicketHandler(ITicketRepository repository)
    : IQueryHandler<ObterTicketQuery, TicketDto?>
{
    public async Task<TicketDto?> Handle(ObterTicketQuery query, CancellationToken cancellationToken)
    {
        Ticket? ticket = await repository.ObterAsync(query.Id, cancellationToken);
        return ticket is null ? null : new TicketDto(
            ticket.Id,
            ticket.NumeroExterno,
            ticket.ChecklistRespondidoEm,
            ticket.Checklist.ToDictionary(item => item.Nome, item => item.Resultado));
    }
}

public sealed class ResponderChecklistHandler(ITicketRepository repository, IClock clock)
    : ICommandHandler<ResponderChecklistCommand>
{
    public async Task Handle(ResponderChecklistCommand command, CancellationToken cancellationToken)
    {
        Ticket ticket = await repository.ObterAsync(command.TicketId, cancellationToken)
            ?? throw new KeyNotFoundException("Ticket não encontrado.");
        ticket.ResponderChecklist(command.Respostas, clock.UtcNow);
        Guid integracaoId = Guid.NewGuid();
        ResultadoChecklistEmpresaAV1 evento = new(
            integracaoId,
            ticket.Id,
            ticket.Checklist.ToDictionary(item => item.Nome, item => item.Resultado!.Value.ToString()),
            command.CorrelationId,
            clock.UtcNow);
        Integracao integracao = new(
            integracaoId,
            "ResultadoChecklistEmpresaA.v1",
            "EmpresaA",
            JsonSerializer.Serialize(evento),
            command.CorrelationId);
        await repository.SalvarChecklistEIntegracaoAsync(ticket, integracao, cancellationToken);
    }
}
