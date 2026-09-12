namespace Backend.Application.Abstractions;

public interface IClock
{
    public DateTimeOffset UtcNow { get; }
}
