using Backend.App.Abstractions.Persistence;
using Backend.Infra.Postgres.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Infra.Postgres;

public static class DependencyInjection
{
    public static IServiceCollection AddPostgres(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<BackendDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<ICadastroRepository, CadastroRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IIntegracaoRepository, IntegracaoRepository>();
        return services;
    }
}
