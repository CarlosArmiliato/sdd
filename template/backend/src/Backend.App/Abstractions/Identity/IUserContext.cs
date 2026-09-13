namespace Backend.App.Abstractions.Identity;

public interface IUserContext
{
    public bool IsAuthenticated { get; }
    public bool IsApplication { get; }
    public string ActorId { get; }
    public Guid? TenantId { get; }
    public Guid? ObjectId { get; }
    public Guid? ClientId { get; }
    public string Username { get; }
    public IReadOnlyCollection<string> Roles { get; }
    public IReadOnlyCollection<string> Scopes { get; }
}
