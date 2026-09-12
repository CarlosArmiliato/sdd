namespace Backend.Contracts.Events;

public sealed record TicketRecebidoV1(
    string EventId,
    string NumeroTicket,
    string CorrelationId,
    DateTimeOffset RecebidoEm);
