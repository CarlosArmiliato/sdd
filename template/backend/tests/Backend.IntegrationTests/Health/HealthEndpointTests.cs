using System.Net;
using System.Net.Http.Json;
using Backend.Domain.Health;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Backend.IntegrationTests.Health;

public sealed class HealthEndpointTests
{
    [Fact]
    public async Task GetHealthReturnsHealthyResponse()
    {
        await using WebApplicationFactory<Program> application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ExternalServices:Enabled", "false");
                builder.ConfigureLogging(logging => logging.ClearProviders());
            });
        using HttpClient client = application.CreateClient();
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        HttpResponseMessage response = await client.GetAsync("/health", cancellationToken);
        ServiceHealth? health = await response.Content
            .ReadFromJsonAsync<ServiceHealth>(cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(health);
        Assert.Equal("healthy", health.Status);
        Assert.NotEqual(default, health.Timestamp);
    }
}
