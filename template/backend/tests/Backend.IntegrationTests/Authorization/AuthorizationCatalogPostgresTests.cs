using Backend.Domain.Authorization;
using Backend.Infra.Postgres.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.IntegrationTests.Authorization;

public sealed class AuthorizationCatalogPostgresTests
{
    [Fact]
    public async Task Ti08RejeitaObjectIdReutilizadoEntreFornecedores()
    {
        string? connectionString = Environment.GetEnvironmentVariable("BACKEND_TEST_POSTGRES_CONNECTION");
        Assert.SkipUnless(!string.IsNullOrWhiteSpace(connectionString), "Requer PostgreSQL isolado em BACKEND_TEST_POSTGRES_CONNECTION.");
        await using BackendDbContext context = CreateContext(connectionString);
        await context.Database.MigrateAsync(TestContext.Current.CancellationToken);
        Guid tenantId = Guid.NewGuid();
        Guid objectId = Guid.NewGuid();
        context.IdentidadesAplicacao.Add(CreateSupplier(tenantId, objectId, Guid.NewGuid(), "XPTO"));
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        context.IdentidadesAplicacao.Add(CreateSupplier(tenantId, objectId, Guid.NewGuid(), "OUTRO"));
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    private static BackendDbContext CreateContext(string? connectionString)
    {
        DbContextOptions<BackendDbContext> options = new DbContextOptionsBuilder<BackendDbContext>()
            .UseNpgsql(connectionString!)
            .Options;
        return new BackendDbContext(options);
    }

    private static IdentidadeAplicacao CreateSupplier(Guid tenantId, Guid objectId, Guid clientId, string supplierCode) =>
        new(Guid.NewGuid(), tenantId, objectId, clientId, IdentidadeAplicacaoTipo.Fornecedor, $"Fornecedor {supplierCode}", supplierCode);
}
