using Firmware.Application.Health;

namespace Firmware.Api.Endpoints;

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
