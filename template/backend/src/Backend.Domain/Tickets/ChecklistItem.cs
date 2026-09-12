namespace Backend.Domain.Tickets;

public sealed class ChecklistItem : Backend.Domain.Auditing.AuditableEntity, Backend.Domain.Auditing.ITouchesParent<Ticket, Guid>
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid TicketId { get; private set; }
    public Guid ParentId => TicketId;
    public string Nome { get; private set; } = string.Empty;
    public ChecklistResultado? Resultado { get; private set; }

    private ChecklistItem() { }

    internal ChecklistItem(Guid ticketId, string nome)
    {
        TicketId = ticketId;
        Nome = nome;
    }

    internal void Responder(ChecklistResultado resultado) => Resultado = resultado;
}
