namespace Backend.App.Abstractions.Identity;

public interface IUserContext
{
    public bool IsAuthenticated { get; }
    public bool IsApplication { get; }
    public string ActorId { get; }
    public string? TenantId { get; }
    public string Username { get; }
    public IReadOnlyCollection<string> Roles { get; }
    public IReadOnlyCollection<string> Scopes { get; }
}
