using Backend.Domain.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Backend.Infra.Postgres.Persistence.Auditing;

internal static class AuditableEntryWriter
{
    public static bool CanAudit(EntityEntry entry) => entry.Entity is IAuditableEntity;

    public static void Create(EntityEntry entry, DateTimeOffset timestamp, string username)
    {
        Set(entry, nameof(IAuditableEntity.CreationTime), timestamp);
        Set(entry, nameof(IAuditableEntity.CreatorUsername), username);
        Set(entry, nameof(IAuditableEntity.ModificationTime), timestamp);
        Set(entry, nameof(IAuditableEntity.ModifierUsername), username);
    }

    public static void Modify(EntityEntry entry, DateTimeOffset timestamp, string username)
    {
        Preserve(entry, nameof(IAuditableEntity.CreationTime));
        Preserve(entry, nameof(IAuditableEntity.CreatorUsername));
        Touch(entry, timestamp, username);
    }

    public static void Touch(EntityEntry entry, DateTimeOffset timestamp, string username)
    {
        SetModified(entry, nameof(IAuditableEntity.ModificationTime), timestamp);
        SetModified(entry, nameof(IAuditableEntity.ModifierUsername), username);
    }

    private static void Preserve(EntityEntry entry, string propertyName)
    {
        PropertyEntry property = entry.Property(propertyName);
        property.CurrentValue = property.OriginalValue;
        property.IsModified = false;
    }

    private static void Set(EntityEntry entry, string propertyName, object value) =>
        entry.Property(propertyName).CurrentValue = value;

    private static void SetModified(EntityEntry entry, string propertyName, object value)
    {
        PropertyEntry property = entry.Property(propertyName);
        property.CurrentValue = value;
        property.IsModified = entry.State != EntityState.Added;
    }
}
