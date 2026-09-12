using Microsoft.Extensions.DependencyInjection;

namespace Backend.Infra.Redis;

public static class DependencyInjection
{
    public static IServiceCollection AddRedis(this IServiceCollection services, string connectionString)
    {
        services.AddStackExchangeRedisCache(options => options.Configuration = connectionString);
        return services;
    }
}
