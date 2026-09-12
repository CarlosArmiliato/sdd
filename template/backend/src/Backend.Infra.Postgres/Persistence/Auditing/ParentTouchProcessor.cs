using Backend.Domain.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Backend.Infra.Postgres.Persistence.Auditing;

internal static class ParentTouchProcessor
{
    private static readonly Type TouchContract = typeof(ITouchesParent<,>);

    public static void Apply(
        DbContext dbContext,
        IEnumerable<EntityEntry> changes,
        DateTimeOffset timestamp,
        string username)
    {
        HashSet<ParentReference> touched = [];
        foreach (EntityEntry childEntry in changes)
        {
            foreach (Type contract in FindContracts(childEntry.Metadata.ClrType))
            {
                TouchParent(dbContext, childEntry, contract, timestamp, username, touched);
            }
        }
    }

    private static void TouchParent(
        DbContext dbContext,
        EntityEntry childEntry,
        Type contract,
        DateTimeOffset timestamp,
        string username,
        ISet<ParentReference> touched)
    {
        Type parentType = contract.GenericTypeArguments[0];
        IForeignKey foreignKey = FindForeignKey(childEntry.Metadata, parentType);
        object parentId = ReadParentId(childEntry, contract, foreignKey);
        ParentReference reference = new(parentType, parentId);
        if (!touched.Add(reference))
        {
            return;
        }
        EntityEntry parentEntry = FindTrackedParent(dbContext, foreignKey, parentId)
            ?? AttachParentStub(dbContext, foreignKey, parentId);
        if (parentEntry.State is not EntityState.Added and not EntityState.Deleted)
        {
            AuditableEntryWriter.Touch(parentEntry, timestamp, username);
        }
    }

    private static Type[] FindContracts(Type childType) => childType.GetInterfaces()
        .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == TouchContract)
        .ToArray();

    private static IForeignKey FindForeignKey(IEntityType childType, Type parentType) =>
        childType.GetForeignKeys().Single(foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == parentType &&
            foreignKey.PrincipalKey.IsPrimaryKey() &&
            foreignKey.Properties.Count == 1);

    private static object ReadParentId(EntityEntry childEntry, Type contract, IForeignKey foreignKey)
    {
        object? mappedId = childEntry.Property(foreignKey.Properties[0].Name).CurrentValue;
        object? declaredId = contract.GetProperty(nameof(ITouchesParent<object, object>.ParentId))
            ?.GetValue(childEntry.Entity);
        if (mappedId is null || !Equals(mappedId, declaredId))
        {
            throw new InvalidOperationException($"{childEntry.Metadata.DisplayName()} expõe ParentId diferente da FK mapeada.");
        }
        return mappedId;
    }

    private static EntityEntry? FindTrackedParent(DbContext dbContext, IForeignKey foreignKey, object parentId) =>
        dbContext.ChangeTracker.Entries()
            .FirstOrDefault(entry =>
                foreignKey.PrincipalEntityType.ClrType.IsInstanceOfType(entry.Entity) &&
                Equals(entry.Property(foreignKey.PrincipalKey.Properties[0].Name).CurrentValue, parentId));

    private static EntityEntry AttachParentStub(DbContext dbContext, IForeignKey foreignKey, object parentId)
    {
        Type parentType = foreignKey.PrincipalEntityType.ClrType;
        object parent = Activator.CreateInstance(parentType, nonPublic: true)
            ?? throw new InvalidOperationException($"Não foi possível criar uma referência de {parentType.Name}.");
        EntityEntry entry = dbContext.Entry(parent);
        entry.Property(foreignKey.PrincipalKey.Properties[0].Name).CurrentValue = parentId;
        entry.State = EntityState.Unchanged;
        return entry;
    }

    private sealed record ParentReference(Type ParentType, object ParentId);
}
