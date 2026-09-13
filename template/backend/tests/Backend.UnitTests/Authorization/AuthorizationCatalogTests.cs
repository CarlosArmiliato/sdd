using Backend.App.Abstractions.Authorization;
using Backend.Domain.Authorization;

namespace Backend.UnitTests.Authorization;

public sealed class AuthorizationCatalogTests
{
    [Fact]
    public void Tu04RoleDaAplicacaoConcedeSomentePermissaoDoPerfil()
    {
        MapeamentoRolePerfil mapping = new(Guid.NewGuid(), "Cadastros.Integration.Write", Guid.NewGuid(), RoleSubjectType.Application);
        AuthorizationGrant grant = CreateGrant(["Cadastros.Write"], []);
        Assert.True(mapping.AplicaPara(application: true));
        Assert.False(mapping.AplicaPara(application: false));
        Assert.True(grant.HasPermission("Cadastros.Write"));
        Assert.False(grant.HasPermission("Cadastros.Delete"));
    }

    [Fact]
    public void Tu05EscopoDeFilialRestringeAcesso()
    {
        MapeamentoRoleEscopo mapping = new(Guid.NewGuid(), "Scope.Branch.FilialA", "Filial", "FilialA");
        AuthorizationGrant grant = CreateGrant([], [new AuthorizationScope(mapping.ScopeType, mapping.ScopeValue)]);
        Assert.True(grant.HasScope("Filial", "FilialA"));
        Assert.False(grant.HasScope("Filial", "FilialB"));
    }

    [Fact]
    public void Tu07IdentidadeFornecedorAtivaECompativelEReconhecida()
    {
        Guid tenantId = Guid.NewGuid();
        Guid objectId = Guid.NewGuid();
        Guid clientId = Guid.NewGuid();
        IdentidadeAplicacao identity = new(Guid.NewGuid(), tenantId, objectId, clientId, IdentidadeAplicacaoTipo.Fornecedor, "Fornecedor XPTO", "XPTO");
        Assert.True(identity.CorrespondeA(tenantId, objectId, clientId));
        Assert.Equal("XPTO", identity.FornecedorCodigo);
    }

    [Fact]
    public void Tu08IdentidadeInativaOuComClaimDivergenteERejeitada()
    {
        Guid tenantId = Guid.NewGuid();
        Guid objectId = Guid.NewGuid();
        Guid clientId = Guid.NewGuid();
        IdentidadeAplicacao identity = new(Guid.NewGuid(), tenantId, objectId, clientId, IdentidadeAplicacaoTipo.Fornecedor, "Fornecedor XPTO", "XPTO");
        identity.DefinirAtiva(false);
        Assert.False(identity.CorrespondeA(tenantId, objectId, clientId));
        identity.DefinirAtiva(true);
        Assert.False(identity.CorrespondeA(tenantId, objectId, Guid.NewGuid()));
    }

    private static AuthorizationGrant CreateGrant(string[] permissions, AuthorizationScope[] scopes) =>
        new(permissions.ToHashSet(StringComparer.Ordinal), scopes.ToHashSet(), null);
}
