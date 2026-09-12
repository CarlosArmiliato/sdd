using Backend.App.Abstractions;

namespace Backend.App.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => TimeProvider.System.GetUtcNow();
}
