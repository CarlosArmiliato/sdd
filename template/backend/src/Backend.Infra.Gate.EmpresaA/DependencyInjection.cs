using Backend.App.Abstractions.Integrations;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Infra.Gate.EmpresaA;

public static class DependencyInjection
{
    public static IServiceCollection AddEmpresaAGate(this IServiceCollection services)
    {
        services.AddScoped<IIntegracaoHandler, ResultadoChecklistEmpresaAHandler>();
        return services;
    }
}
