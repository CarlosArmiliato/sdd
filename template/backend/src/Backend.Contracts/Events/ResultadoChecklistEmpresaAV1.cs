namespace Backend.Contracts.Events;

public sealed record ResultadoChecklistEmpresaAV1(
    Guid IntegracaoId,
    Guid TicketId,
    IReadOnlyDictionary<string, string> Itens,
    string CorrelationId,
    DateTimeOffset RespondidoEm);
