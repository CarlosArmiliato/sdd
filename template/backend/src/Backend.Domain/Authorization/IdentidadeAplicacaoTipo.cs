using System.ComponentModel;

namespace Backend.Domain.Authorization;

public enum IdentidadeAplicacaoTipo
{
    [Description("Aplicação interna")]
    Interna = 1,

    [Description("Fornecedor")]
    Fornecedor = 2
}
