namespace Backend.App.Abstractions.Identity;

public interface IUserContext
{
    bool IsAuthenticated { get; }
    bool IsApplication { get; }
    string ActorId { get; }
    string? TenantId { get; }
    string Username { get; }
    IReadOnlyCollection<string> Roles { get; }
    IReadOnlyCollection<string> Scopes { get; }
}
