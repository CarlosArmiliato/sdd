using Backend.App.Abstractions.Identity;
using Backend.App.Identity;

namespace Backend.Infra.Postgres.Persistence.Auditing;

public static class AuditActorIdResolver
{
    public static string ForPersistence(IUserContext userContext)
    {
        ArgumentNullException.ThrowIfNull(userContext);
        string actorId = ActorId.Parse(userContext.ActorId).Value;
        return actorId.StartsWith("v1:", StringComparison.Ordinal)
            ? actorId
            : throw new InvalidOperationException("A auditoria requer um ActorId versionado.");
    }

    public static string ForRead(string storedValue) => ActorId.Parse(storedValue).Value;
}
