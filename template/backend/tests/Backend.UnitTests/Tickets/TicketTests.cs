using Backend.Domain.Tickets;

namespace Backend.UnitTests.Tickets;

public sealed class TicketTests
{
    [Fact]
    public void NovoTicketPossuiChecklistPadrao()
    {
        Ticket ticket = new("T-123", "corr-123");
        Assert.Equal(["Item1", "Item2", "Item3"], ticket.Checklist.Select(x => x.Nome));
        Assert.Equal("corr-123", ticket.CorrelationId);
        Assert.Equal("T-123", ticket.NumeroExterno);
    }

    [Fact]
    public void ResponderChecklistExigeTodosOsItens()
    {
        Ticket ticket = new("T-123", "corr-123");
        Assert.Throws<InvalidOperationException>(() => ticket.ResponderChecklist(
            new Dictionary<string, ChecklistResultado> { ["Item1"] = ChecklistResultado.Conforme },
            DateTimeOffset.UtcNow));
        Assert.All(ticket.Checklist, item => Assert.Null(item.Resultado));
    }

    [Fact]
    public void ResponderChecklistAtualizaTodosOsItens()
    {
        DateTimeOffset respondidoEm = DateTimeOffset.UtcNow;
        Ticket ticket = new("T-123", "corr-123");
        Dictionary<string, ChecklistResultado> respostas = new()
        {
            ["Item1"] = ChecklistResultado.Conforme,
            ["Item2"] = ChecklistResultado.NaoConforme,
            ["Item3"] = ChecklistResultado.Conforme
        };
        ticket.ResponderChecklist(respostas, respondidoEm);
        Assert.Equal(respondidoEm, ticket.ChecklistRespondidoEm);
        Assert.Equal(respostas.Values, ticket.Checklist.Select(x => x.Resultado!.Value));
    }
}
