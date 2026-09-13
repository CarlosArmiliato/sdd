using System.ComponentModel;

namespace Backend.Domain.Authorization;

public enum RoleSubjectType
{
    [Description("Usuário")]
    User = 1,

    [Description("Aplicação")]
    Application = 2,

    [Description("Usuário ou aplicação")]
    Both = 3
}
