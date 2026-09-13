using Backend.App.Abstractions.Identity;

namespace Backend.App.Identity;

public sealed class SystemUserContext(string actorId) : IUserContext
{
    private static readonly string[] EmptyClaims = [];

    public bool IsAuthenticated => false;
    public bool IsApplication => true;
    public string ActorId { get; } = Backend.App.Identity.ActorId.Parse(actorId).Value;
    public Guid? TenantId => null;
    public Guid? ObjectId => null;
    public Guid? ClientId => null;
    public string Username => ActorId;
    public IReadOnlyCollection<string> Roles => EmptyClaims;
    public IReadOnlyCollection<string> Scopes => EmptyClaims;
}
