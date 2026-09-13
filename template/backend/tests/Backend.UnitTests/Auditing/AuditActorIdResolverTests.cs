using Backend.App.Abstractions.Identity;
using Backend.Infra.Postgres.Persistence.Auditing;

namespace Backend.UnitTests.Auditing;

public sealed class AuditActorIdResolverTests
{
    [Fact]
    public void Tu09PersisteActorIdVersionado()
    {
        const string actorId = "v1:entra:user:11111111-1111-1111-1111-111111111111:22222222-2222-2222-2222-222222222222";
        string resolved = AuditActorIdResolver.ForPersistence(new TestUserContext(actorId));
        Assert.Equal(actorId, resolved);
        Assert.DoesNotContain("@", resolved);
    }

    [Fact]
    public void Tu10PreservaValorLegadoParaLeitura()
    {
        const string legacyValue = "usuario.legado@empresa.example";
        string resolved = AuditActorIdResolver.ForRead(legacyValue);
        Assert.Equal(legacyValue, resolved);
    }

    [Fact]
    public void RejeitaIdentidadeLegadaEmNovaGravacao()
    {
        Assert.Throws<InvalidOperationException>(() => AuditActorIdResolver.ForPersistence(new TestUserContext("usuario.legado@empresa.example")));
    }

    private sealed class TestUserContext(string actorId) : IUserContext
    {
        public bool IsAuthenticated => true;
        public bool IsApplication => false;
        public string ActorId => actorId;
        public Guid? TenantId => null;
        public Guid? ObjectId => null;
        public Guid? ClientId => null;
        public string Username => actorId;
        public IReadOnlyCollection<string> Roles => [];
        public IReadOnlyCollection<string> Scopes => [];
    }
}
