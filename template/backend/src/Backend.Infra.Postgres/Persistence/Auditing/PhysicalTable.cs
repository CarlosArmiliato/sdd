using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Backend.Infra.Postgres.Persistence.Auditing;

internal readonly record struct PhysicalTable(string? Schema, string Name)
{
    public StoreObjectIdentifier StoreObject => StoreObjectIdentifier.Table(Name, Schema);

    public static PhysicalTable? From(IReadOnlyEntityType entityType)
    {
        string? tableName = entityType.GetTableName();
        return tableName is null ? null : new(entityType.GetSchema(), tableName);
    }
}
