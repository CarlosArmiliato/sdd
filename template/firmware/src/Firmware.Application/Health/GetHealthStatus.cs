using Firmware.Application.Abstractions;
using Firmware.Domain.Health;

namespace Firmware.Application.Health;

public sealed class GetHealthStatus(IClock clock)
{
    public ServiceHealth Handle()
    {
        return ServiceHealth.Healthy(clock.UtcNow);
    }
}
