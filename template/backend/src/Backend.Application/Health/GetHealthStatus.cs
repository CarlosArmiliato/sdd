using Backend.Application.Abstractions;
using Backend.Domain.Health;

namespace Backend.Application.Health;

public sealed class GetHealthStatus(IClock clock)
{
    public ServiceHealth Handle()
    {
        return ServiceHealth.Healthy(clock.UtcNow);
    }
}
