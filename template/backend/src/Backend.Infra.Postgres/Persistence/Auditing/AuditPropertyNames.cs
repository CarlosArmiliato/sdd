using Backend.Domain.Auditing;

namespace Backend.Infra.Postgres.Persistence.Auditing;

internal static class AuditPropertyNames
{
    public static readonly string[] All =
    [
        nameof(IAuditableEntity.CreationTime),
        nameof(IAuditableEntity.CreatorUsername),
        nameof(IAuditableEntity.ModificationTime),
        nameof(IAuditableEntity.ModifierUsername)
    ];
}
