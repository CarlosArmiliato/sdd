using Backend.Domain.Authorization;
using Backend.Infra.Postgres.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Backend.UnitTests.Authorization;

public sealed class AuthorizationMappingsTests
{
    [Fact]
    public void CatalogoPossuiIndicesUnicosESeedMinimo()
    {
        using BackendDbContext context = CreateContext();
        IModel model = context.GetService<IDesignTimeModel>().Model;
        AssertUniqueIndex<IdentidadeAplicacao>(model, nameof(IdentidadeAplicacao.TenantId), nameof(IdentidadeAplicacao.ObjectId));
        AssertUniqueIndex<IdentidadeAplicacao>(model, nameof(IdentidadeAplicacao.TenantId), nameof(IdentidadeAplicacao.ClientId));
        Assert.Equal(7, model.FindEntityType(typeof(Permissao))!.GetSeedData().Count());
    }

    private static BackendDbContext CreateContext()
    {
        DbContextOptions<BackendDbContext> options = new DbContextOptionsBuilder<BackendDbContext>()
            .UseNpgsql("Host=localhost;Database=backend_authorization_mappings")
            .Options;
        return new BackendDbContext(options);
    }

    private static void AssertUniqueIndex<TEntity>(IModel model, params string[] propertyNames)
    {
        IEntityType entityType = model.FindEntityType(typeof(TEntity))!;
        Assert.Contains(entityType.GetIndexes(), index => index.IsUnique && index.Properties.Select(property => property.Name).SequenceEqual(propertyNames));
    }
}
