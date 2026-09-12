namespace Firmware.Domain.Health;

public sealed record ServiceHealth(string Status, DateTimeOffset Timestamp)
{
    private const string HealthyStatus = "healthy";

    public static ServiceHealth Healthy(DateTimeOffset timestamp)
    {
        return new ServiceHealth(HealthyStatus, timestamp);
    }
}
