namespace Backend.App.Identity;

public sealed class ActorContext
{
    public ActorContext(
        ActorId actorIdentity,
        ActorType actorType,
        EntraTenantId? tenantId,
        EntraObjectId? objectId,
        EntraClientId? clientId,
        IEnumerable<string>? roles = null,
        IEnumerable<string>? scopes = null,
        string? originatorActorId = null)
    {
        ActorIdentity = actorIdentity ?? throw new ArgumentNullException(nameof(actorIdentity));
        ActorType = actorType;
        TenantId = tenantId;
        ObjectId = objectId;
        ClientId = clientId;
        Roles = Normalize(roles);
        Scopes = Normalize(scopes);
        OriginatorActorId = originatorActorId;
    }

    public ActorId ActorIdentity { get; }
    public string ActorId => ActorIdentity.Value;
    public ActorType ActorType { get; }
    public EntraTenantId? TenantId { get; }
    public EntraObjectId? ObjectId { get; }
    public EntraClientId? ClientId { get; }
    public IReadOnlyCollection<string> Roles { get; }
    public IReadOnlyCollection<string> Scopes { get; }
    public string? OriginatorActorId { get; }

    public static ActorContext CreateUser(
        EntraTenantId tenantId,
        EntraObjectId objectId,
        IEnumerable<string>? roles = null,
        IEnumerable<string>? scopes = null) =>
        new(Backend.App.Identity.ActorId.CreateUser(tenantId, objectId), ActorType.User, tenantId, objectId, null, roles, scopes);

    public static ActorContext CreateApplication(
        EntraTenantId tenantId,
        EntraObjectId objectId,
        EntraClientId clientId,
        ActorType actorType,
        IEnumerable<string>? roles = null) =>
        IsApplication(actorType)
            ? new(Backend.App.Identity.ActorId.CreateApplication(tenantId, objectId), actorType, tenantId, objectId, clientId, roles)
            : throw new ArgumentException("O tipo de ator não representa uma aplicação.", nameof(actorType));

    public static ActorContext CreateSystem(string host, string process) =>
        new(Backend.App.Identity.ActorId.CreateSystem(host, process), ActorType.BackgroundJob, null, null, null);

    private static string[] Normalize(IEnumerable<string>? values) =>
        values?.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.Ordinal).ToArray() ?? [];

    private static bool IsApplication(ActorType actorType) =>
        actorType is ActorType.InternalApplication or ActorType.SupplierApplication;
}
