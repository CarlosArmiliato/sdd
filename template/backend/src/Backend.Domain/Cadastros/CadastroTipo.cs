using System.ComponentModel;

namespace Backend.Domain.Cadastros;

public enum CadastroTipo
{
    [Description("Fazenda")]
    Fazenda = 1,

    [Description("Ano agrícola")]
    AnoAgricola = 2,

    [Description("Safra")]
    Safra = 3,

    [Description("Cultura")]
    Cultura = 4
}
