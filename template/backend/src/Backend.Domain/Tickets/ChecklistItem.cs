namespace Backend.Domain.Tickets;

public sealed class ChecklistItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Nome { get; private set; } = string.Empty;
    public ChecklistResultado? Resultado { get; private set; }

    private ChecklistItem() { }

    internal ChecklistItem(string nome) => Nome = nome;

    internal void Responder(ChecklistResultado resultado) => Resultado = resultado;
}
