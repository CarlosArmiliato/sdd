using System.ComponentModel;

namespace Backend.Domain.Tickets;

public enum ChecklistResultado
{
    [Description("Conforme")]
    Conforme = 1,

    [Description("Não conforme")]
    NaoConforme = 2
}
