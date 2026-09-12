namespace Backend.Domain.Tickets;

public sealed class Ticket : Backend.Domain.Auditing.AuditableEntity
{
    private readonly List<ChecklistItem> _checklist = [];

    public Guid Id { get; private set; }
    public string NumeroExterno { get; private set; } = string.Empty;
    public string CorrelationId { get; private set; } = string.Empty;
    public DateTimeOffset? ChecklistRespondidoEm { get; private set; }
    public IReadOnlyCollection<ChecklistItem> Checklist => _checklist;

    private Ticket() { }

    public Ticket(string numeroExterno, string correlationId)
    {
        Id = Guid.NewGuid();
        NumeroExterno = numeroExterno;
        CorrelationId = correlationId;
        _checklist.AddRange([new(Id, "Item1"), new(Id, "Item2"), new(Id, "Item3")]);
    }

    public void ResponderChecklist(
        IReadOnlyDictionary<string, ChecklistResultado> respostas,
        DateTimeOffset respondidoEm)
    {
        if (_checklist.Any(item => !respostas.ContainsKey(item.Nome)))
        {
            throw new InvalidOperationException("Todos os itens do checklist devem ser respondidos.");
        }
        foreach (ChecklistItem item in _checklist)
        {
            ChecklistResultado resultado = respostas[item.Nome];
            item.Responder(resultado);
        }

        ChecklistRespondidoEm = respondidoEm;
    }
}
