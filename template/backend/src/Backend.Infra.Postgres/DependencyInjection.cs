using Backend.App.Abstractions.Persistence;
using Backend.Infra.Postgres.Persistence;
using Backend.Infra.Postgres.Persistence.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Backend.Infra.Postgres;

public static class DependencyInjection
{
    public static IServiceCollection AddPostgres(this IServiceCollection services, string connectionString)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<AuditableSaveChangesInterceptor>();
        services.AddDbContext<BackendDbContext>((provider, options) => options
            .UseNpgsql(connectionString)
            .AddInterceptors(provider.GetRequiredService<AuditableSaveChangesInterceptor>()));
        services.AddScoped<ICadastroRepository, CadastroRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IIntegracaoRepository, IntegracaoRepository>();
        return services;
    }
}
