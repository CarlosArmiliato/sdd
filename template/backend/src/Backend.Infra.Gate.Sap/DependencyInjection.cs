using Backend.App.Abstractions.Integrations;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Infra.Gate.Sap;

public static class DependencyInjection
{
    public static IServiceCollection AddSapGate(this IServiceCollection services)
    {
        services.AddHttpClient<ISapCadastroSource, SapGateway>((provider, client) =>
            client.BaseAddress = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<SapOptions>>().Value.BaseAddress)
            .AddStandardResilienceHandler();
        return services;
    }
}
