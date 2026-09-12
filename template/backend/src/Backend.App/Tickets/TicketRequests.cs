using Backend.Contracts.Events;
using Backend.Domain.Tickets;
using Cortex.Mediator.Commands;
using Cortex.Mediator.Queries;

namespace Backend.App.Tickets;

public sealed record TicketDto(
    Guid Id,
    string NumeroExterno,
    DateTimeOffset? ChecklistRespondidoEm,
    IReadOnlyDictionary<string, ChecklistResultado?> Checklist);

public sealed record ReceberTicketCommand(TicketRecebidoV1 Evento) : ICommand<Guid>;
public sealed record ObterTicketQuery(Guid Id) : IQuery<TicketDto?>;
public sealed record ResponderChecklistCommand(
    Guid TicketId,
    IReadOnlyDictionary<string, ChecklistResultado> Respostas,
    string CorrelationId) : ICommand;
