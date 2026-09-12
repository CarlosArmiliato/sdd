namespace Backend.App.Abstractions;

public interface IClock
{
    public DateTimeOffset UtcNow { get; }
}
