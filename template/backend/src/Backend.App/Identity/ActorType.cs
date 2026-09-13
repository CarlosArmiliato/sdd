using System.ComponentModel;

namespace Backend.App.Identity;

public enum ActorType
{
    [Description("Usuário interno autenticado pelo Microsoft Entra ID")]
    User = 1,

    [Description("Aplicação interna autenticada pelo Microsoft Entra ID")]
    InternalApplication = 2,

    [Description("Aplicação de fornecedor autenticada pelo Microsoft Entra ID")]
    SupplierApplication = 3,

    [Description("Processo em segundo plano ou execução local controlada")]
    BackgroundJob = 4
}
