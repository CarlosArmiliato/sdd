using System.ComponentModel;

namespace Backend.Domain.Integracoes;

public enum IntegracaoStatus
{
    [Description("Pendente")]
    Pendente = 1,

    [Description("Processando")]
    Processando = 2,

    [Description("Concluída")]
    Concluida = 3,

    [Description("Falha definitiva")]
    FalhaDefinitiva = 4
}
