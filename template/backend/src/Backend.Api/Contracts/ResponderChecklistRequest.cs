using Backend.Domain.Tickets;

namespace Backend.Api.Contracts;

public sealed record ResponderChecklistRequest(
    IReadOnlyDictionary<string, ChecklistResultado> Respostas,
    string CorrelationId);
