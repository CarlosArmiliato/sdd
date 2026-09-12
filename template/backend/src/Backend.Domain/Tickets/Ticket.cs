namespace Backend.Domain.Tickets;

public enum ChecklistResultado
{
    Conforme,
    NaoConforme
}

public sealed class Ticket
{
    private readonly List<ChecklistItem> _checklist = [];
    private Ticket() { }

    public Ticket(string numeroExterno, string correlationId)
    {
        Id = Guid.NewGuid();
        NumeroExterno = numeroExterno;
        CorrelationId = correlationId;
        _checklist.AddRange([new("Item1"), new("Item2"), new("Item3")]);
    }

    public Guid Id { get; private set; }
    public string NumeroExterno { get; private set; } = string.Empty;
    public string CorrelationId { get; private set; } = string.Empty;
    public DateTimeOffset? ChecklistRespondidoEm { get; private set; }
    public IReadOnlyCollection<ChecklistItem> Checklist => _checklist;

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

public sealed class ChecklistItem
{
    private ChecklistItem() { }
    internal ChecklistItem(string nome) => Nome = nome;

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Nome { get; private set; } = string.Empty;
    public ChecklistResultado? Resultado { get; private set; }

    internal void Responder(ChecklistResultado resultado) => Resultado = resultado;
}
