using Backend.App.Abstractions.Identity;
using Backend.Domain.Cadastros;
using Backend.Infra.Postgres.Persistence;
using Backend.Infra.Postgres.Persistence.Auditing;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Backend.IntegrationTests.Auditing;

public sealed class AuditingPostgresTests
{
    private const string ActorId = "v1:entra:user:11111111-1111-1111-1111-111111111111:22222222-2222-2222-2222-222222222222";
    private const string LegacyActorId = "usuario.legado@empresa.example";

    [Fact]
    public async Task Ti03PersisteActorIdEPreservaCriadorLegado()
    {
        string connectionString = GetConnectionString();
        await using BackendDbContext context = CreateContext(connectionString);
        await context.Database.EnsureDeletedAsync(TestContext.Current.CancellationToken);
        await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
        Cadastro cadastro = new(CadastroTipo.Fazenda, "AUD-001", "Cadastro auditado");
        context.Cadastros.Add(cadastro);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        AssertAuditedWithActor(cadastro);
        await context.Database.ExecuteSqlInterpolatedAsync($"UPDATE \"Cadastros\" SET \"CreatorUsername\" = {LegacyActorId}, \"ModifierUsername\" = {LegacyActorId} WHERE \"Id\" = {cadastro.Id}", TestContext.Current.CancellationToken);
        context.ChangeTracker.Clear();
        Cadastro legacy = await context.Cadastros.SingleAsync(item => item.Id == cadastro.Id, TestContext.Current.CancellationToken);
        legacy.Atualizar("AUD-001", "Cadastro atualizado");
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        Assert.Equal(LegacyActorId, legacy.CreatorUsername);
        Assert.Equal(ActorId, legacy.ModifierUsername);
    }

    private static BackendDbContext CreateContext(string connectionString)
    {
        DbContextOptions<BackendDbContext> options = new DbContextOptionsBuilder<BackendDbContext>()
            .UseNpgsql(connectionString)
            .AddInterceptors(new AuditableSaveChangesInterceptor(TimeProvider.System, new TestUserContext()))
            .Options;
        return new BackendDbContext(options);
    }

    private static string GetConnectionString()
    {
        string? configured = Environment.GetEnvironmentVariable("BACKEND_TEST_POSTGRES_CONNECTION");
        Assert.SkipUnless(!string.IsNullOrWhiteSpace(configured), "Requer PostgreSQL isolado em BACKEND_TEST_POSTGRES_CONNECTION.");
        return new NpgsqlConnectionStringBuilder(configured!) { Database = "backend_task_2_audit" }.ConnectionString;
    }

    private static void AssertAuditedWithActor(Cadastro cadastro)
    {
        Assert.Equal(ActorId, cadastro.CreatorUsername);
        Assert.Equal(ActorId, cadastro.ModifierUsername);
        Assert.DoesNotContain("@", cadastro.CreatorUsername);
    }

    private sealed class TestUserContext : IUserContext
    {
        public bool IsAuthenticated => true;
        public bool IsApplication => false;
        public string ActorId => AuditingPostgresTests.ActorId;
        public Guid? TenantId => null;
        public Guid? ObjectId => null;
        public Guid? ClientId => null;
        public string Username => "usuario@empresa.example";
        public IReadOnlyCollection<string> Roles => [];
        public IReadOnlyCollection<string> Scopes => [];
    }
}
