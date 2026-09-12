using Backend.Domain.Auditing;
using Backend.Infra.Postgres.Persistence.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infra.Postgres.Persistence;

public static class AuditModelBuilderExtensions
{
    private const int UsernameMaximumLength = 320;
    private const string TimestampColumnType = "timestamp with time zone";

    public static ModelBuilder ApplyMandatoryAuditing(this ModelBuilder modelBuilder)
    {
        foreach (IGrouping<PhysicalTable, IMutableEntityType> table in PhysicalTables(modelBuilder))
        {
            ConfigureTable(modelBuilder, table.Key, table);
        }
        ValidateParentTouchContracts(modelBuilder.Model);
        return modelBuilder;
    }

    private static IEnumerable<IGrouping<PhysicalTable, IMutableEntityType>> PhysicalTables(
        ModelBuilder modelBuilder) =>
        modelBuilder.Model.GetEntityTypes()
            .Where(entity => entity.FindPrimaryKey() is not null)
            .Select(entity => (Entity: entity, Table: PhysicalTable.From(entity)))
            .Where(mapping => mapping.Table is not null)
            .GroupBy(mapping => mapping.Table!.Value, mapping => mapping.Entity);

    private static void ConfigureTable(ModelBuilder modelBuilder, PhysicalTable table, IEnumerable<IMutableEntityType> mappings)
    {
        IMutableEntityType[] entityTypes = mappings.ToArray();
        IMutableEntityType owner = entityTypes
            .Where(IsAuditable)
            .Where(entity => HasAuditColumns(entity, table.StoreObject))
            .OrderBy(entity => entity.FindOwnership() is null ? 0 : 1)
            .FirstOrDefault()
            ?? throw MissingAuditContract(table, entityTypes);
        ConfigureAuditColumns(owner, table.StoreObject);
        ConfigureIncrementalIndex(modelBuilder, owner, table);
    }

    private static bool IsAuditable(IMutableEntityType entityType) =>
        typeof(IAuditableEntity).IsAssignableFrom(entityType.ClrType);

    private static bool HasAuditColumns(IMutableEntityType entityType, StoreObjectIdentifier table) =>
        AuditPropertyNames.All.All(name => entityType.FindProperty(name)?.GetColumnName(table) is not null);

    private static void ConfigureAuditColumns(IMutableEntityType owner, StoreObjectIdentifier table)
    {
        foreach (string propertyName in AuditPropertyNames.All)
        {
            IMutableProperty property = owner.FindProperty(propertyName)!;
            property.SetColumnName(propertyName, table);
            property.IsNullable = false;
        }
        owner.FindProperty(nameof(IAuditableEntity.CreationTime))!.SetColumnType(TimestampColumnType);
        owner.FindProperty(nameof(IAuditableEntity.ModificationTime))!.SetColumnType(TimestampColumnType);
        owner.FindProperty(nameof(IAuditableEntity.CreatorUsername))!.SetMaxLength(UsernameMaximumLength);
        owner.FindProperty(nameof(IAuditableEntity.ModifierUsername))!.SetMaxLength(UsernameMaximumLength);
    }

    private static void ConfigureIncrementalIndex(ModelBuilder modelBuilder, IMutableEntityType owner, PhysicalTable table)
    {
        string[] propertyNames =
        [
            nameof(IAuditableEntity.ModificationTime),
            .. owner.FindPrimaryKey()!.Properties.Select(property => property.Name)
        ];
        IndexBuilder index = modelBuilder.Entity(owner.ClrType).HasIndex(propertyNames);
        string keySuffix = string.Join("_", owner.FindPrimaryKey()!.Properties.Select(property => property.Name));
        index.HasDatabaseName($"IX_{table.Name}_ModificationTime_{keySuffix}");
    }

    private static void ValidateParentTouchContracts(IMutableModel model)
    {
        foreach (IMutableEntityType child in model.GetEntityTypes())
        {
            foreach (Type contract in TouchContracts(child.ClrType))
            {
                ValidateParentTouchContract(model, child, contract);
            }
        }
    }

    private static void ValidateParentTouchContract(
        IMutableModel model,
        IMutableEntityType child,
        Type contract)
    {
        Type parentType = contract.GenericTypeArguments[0];
        Type keyType = contract.GenericTypeArguments[1];
        IMutableEntityType parent = model.FindEntityType(parentType)
            ?? throw new InvalidOperationException($"{parentType.Name} não está mapeado no modelo.");
        IMutableKey primaryKey = parent.FindPrimaryKey()
            ?? throw new InvalidOperationException($"{parentType.Name} não possui chave primária.");
        bool validKey = primaryKey.Properties.Count == 1 && primaryKey.Properties[0].ClrType == keyType;
        bool validForeignKey = child.GetForeignKeys().Count(foreignKey =>
            foreignKey.PrincipalEntityType == parent &&
            foreignKey.PrincipalKey == primaryKey &&
            foreignKey.Properties.Count == 1) == 1;
        if (!validKey || !validForeignKey || !IsAuditable(parent))
        {
            throw new InvalidOperationException($"{child.DisplayName()} possui um contrato ITouchesParent inválido.");
        }
    }

    private static IEnumerable<Type> TouchContracts(Type entityType) => entityType.GetInterfaces()
        .Where(type => type.IsGenericType &&
            type.GetGenericTypeDefinition() == typeof(ITouchesParent<,>));

    private static InvalidOperationException MissingAuditContract(
        PhysicalTable table,
        IEnumerable<IMutableEntityType> entityTypes)
    {
        string mappedTypes = string.Join(", ", entityTypes.Select(entity => entity.DisplayName()));
        return new($"A tabela física {table.Name} não possui os quatro campos de auditoria. Mapeamentos: {mappedTypes}.");
    }
}
