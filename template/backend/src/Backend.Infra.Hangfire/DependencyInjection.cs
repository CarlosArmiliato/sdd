using Backend.App.Abstractions.BackgroundJobs;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Infra.Hangfire;

public static class DependencyInjection
{
    public static IServiceCollection AddHangfireClient(this IServiceCollection services, string connectionString)
    {
        services.AddHangfire(configuration => configuration.UsePostgreSqlStorage(options =>
            options.UseNpgsqlConnection(connectionString)));
        services.AddScoped<IBackgroundCommandScheduler, HangfireBackgroundCommandScheduler>();
        return services;
    }

    public static IServiceCollection AddHangfireWorker(this IServiceCollection services, string connectionString)
    {
        services.AddHangfireClient(connectionString);
        services.AddHangfireServer(options => options.ServerName = $"backend-worker-{Environment.MachineName}");
        return services;
    }
}
