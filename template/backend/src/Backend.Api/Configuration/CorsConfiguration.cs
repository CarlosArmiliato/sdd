namespace Backend.Api.Configuration;

public static class CorsConfiguration
{
    private const string DefaultOrigin = "http://localhost:5173";

    public static IServiceCollection AddApiCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string[] origins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [DefaultOrigin];
        services.AddCors(options => options.AddDefaultPolicy(policy =>
            policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));
        return services;
    }
}
