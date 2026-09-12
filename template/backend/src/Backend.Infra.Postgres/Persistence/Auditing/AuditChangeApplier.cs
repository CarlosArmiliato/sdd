using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Backend.Infra.Postgres.Persistence.Auditing;

internal sealed class AuditChangeApplier(
    DbContext dbContext,
    DateTimeOffset timestamp,
    string username)
{
    private readonly EntityEntry[] _changes = dbContext.ChangeTracker.Entries()
        .Where(IsChanged)
        .ToArray();

    public void Apply()
    {
        ApplyDirectAuditing();
        TouchOwnersOfSharedTables();
        ParentTouchProcessor.Apply(dbContext, _changes, timestamp, username);
    }

    private void ApplyDirectAuditing()
    {
        foreach (EntityEntry entry in _changes.Where(AuditableEntryWriter.CanAudit))
        {
            if (entry.State == EntityState.Added)
            {
                AuditableEntryWriter.Create(entry, timestamp, username);
            }
            else if (entry.State == EntityState.Modified)
            {
                AuditableEntryWriter.Modify(entry, timestamp, username);
            }
        }
    }

    private void TouchOwnersOfSharedTables()
    {
        foreach (EntityEntry entry in _changes.Where(x => !AuditableEntryWriter.CanAudit(x)))
        {
            EntityEntry? owner = FindAuditOwner(entry);
            if (owner is null || owner.State == EntityState.Deleted)
            {
                throw MissingAuditOwner(entry);
            }
            if (owner.State == EntityState.Added)
            {
                AuditableEntryWriter.Create(owner, timestamp, username);
            }
            else
            {
                AuditableEntryWriter.Touch(owner, timestamp, username);
            }
        }
    }

    private EntityEntry? FindAuditOwner(EntityEntry changedEntry)
    {
        PhysicalTable? table = PhysicalTable.From(changedEntry.Metadata);
        return table is null ? null : dbContext.ChangeTracker.Entries()
            .Where(AuditableEntryWriter.CanAudit)
            .Where(entry => PhysicalTable.From(entry.Metadata) == table)
            .OrderBy(entry => entry.Metadata.FindOwnership() is null ? 0 : 1)
            .FirstOrDefault();
    }

    private static bool IsChanged(EntityEntry entry) =>
        entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted;

    private static InvalidOperationException MissingAuditOwner(EntityEntry entry) =>
        new($"A alteração de {entry.Metadata.DisplayName()} não possui entidade auditável rastreada para a tabela física.");
}
