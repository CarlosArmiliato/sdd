using Backend.App.Abstractions.Identity;

namespace Backend.App.Identity;

public sealed class SystemUserContext(string actorId) : IUserContext
{
    private static readonly string[] EmptyClaims = [];

    public bool IsAuthenticated => false;
    public bool IsApplication => true;
    public string ActorId { get; } = RequireActorId(actorId);
    public string? TenantId => null;
    public string Username => ActorId;
    public IReadOnlyCollection<string> Roles => EmptyClaims;
    public IReadOnlyCollection<string> Scopes => EmptyClaims;

    private static string RequireActorId(string value) =>
        !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new ArgumentException("O identificador do processo é obrigatório.", nameof(value));
}
