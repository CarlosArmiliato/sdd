using Backend.Application.Health;

namespace Backend.Api.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/health", (GetHealthStatus query) =>
                TypedResults.Ok(query.Handle()))
            .WithName("GetHealth")
            .WithTags("Health");
        return endpoints;
    }
}
