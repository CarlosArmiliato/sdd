using Backend.Application.Abstractions;
using Backend.Application.Health;

namespace Backend.UnitTests.Health;

public sealed class GetHealthStatusTests
{
    [Fact]
    public void HandleReturnsHealthyStatusWithCurrentTimestamp()
    {
        DateTimeOffset timestamp = new(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);
        GetHealthStatus query = new(new StubClock(timestamp));

        var result = query.Handle();

        Assert.Equal("healthy", result.Status);
        Assert.Equal(timestamp, result.Timestamp);
    }

    private sealed class StubClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
