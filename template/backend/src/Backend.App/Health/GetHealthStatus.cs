using Backend.App.Abstractions;
using Backend.Domain.Health;

namespace Backend.App.Health;

public sealed class GetHealthStatus(IClock clock)
{
    public ServiceHealth Handle()
    {
        return ServiceHealth.Healthy(clock.UtcNow);
    }
}
