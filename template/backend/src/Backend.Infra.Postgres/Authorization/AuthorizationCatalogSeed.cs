using Backend.Domain.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infra.Postgres.Authorization;

internal static class AuthorizationCatalogSeed
{
    private const string SeedActorId = "v1:system:authorization-seed";
    private static readonly DateTimeOffset SeededAt = new(2026, 9, 13, 0, 0, 0, TimeSpan.Zero);
    private static readonly Guid AdministracaoProfileId = Guid.Parse("a0000000-0000-0000-0000-000000000001");
    private static readonly Guid IntegracaoProfileId = Guid.Parse("a0000000-0000-0000-0000-000000000002");

    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PerfilAcesso>().HasData(
            new { Id = AdministracaoProfileId, Codigo = "Administracao", Nome = "Administração do sistema", Ativo = true, CreationTime = SeededAt, CreatorUsername = SeedActorId, ModificationTime = SeededAt, ModifierUsername = SeedActorId },
            new { Id = IntegracaoProfileId, Codigo = "IntegracaoCadastrosEscrita", Nome = "Integração de cadastros com escrita", Ativo = true, CreationTime = SeededAt, CreatorUsername = SeedActorId, ModificationTime = SeededAt, ModifierUsername = SeedActorId });
        modelBuilder.Entity<Permissao>().HasData(CreatePermissions());
        modelBuilder.Entity<PerfilPermissao>().HasData(CreateProfilePermissions());
        modelBuilder.Entity<MapeamentoRolePerfil>().HasData(
            new { Id = Guid.Parse("a0000000-0000-0000-0000-000000000101"), RoleValue = "Administracao", PerfilId = AdministracaoProfileId, SubjectType = RoleSubjectType.Both, Ativo = true, CreationTime = SeededAt, CreatorUsername = SeedActorId, ModificationTime = SeededAt, ModifierUsername = SeedActorId },
            new { Id = Guid.Parse("a0000000-0000-0000-0000-000000000102"), RoleValue = "Cadastros.Integration.Write", PerfilId = IntegracaoProfileId, SubjectType = RoleSubjectType.Application, Ativo = true, CreationTime = SeededAt, CreatorUsername = SeedActorId, ModificationTime = SeededAt, ModifierUsername = SeedActorId });
        modelBuilder.Entity<MapeamentoRoleEscopo>().HasData(
            new { Id = Guid.Parse("a0000000-0000-0000-0000-000000000201"), RoleValue = "Scope.Branch.FilialA", ScopeType = MapeamentoRoleEscopo.FilialScopeType, ScopeValue = "FilialA", Ativo = true, CreationTime = SeededAt, CreatorUsername = SeedActorId, ModificationTime = SeededAt, ModifierUsername = SeedActorId });
    }

    private static object[] CreatePermissions() =>
    [
        CreatePermission("a0000000-0000-0000-0000-000000000301", "Cadastros.Read", "Consultar cadastros"),
        CreatePermission("a0000000-0000-0000-0000-000000000302", "Cadastros.Write", "Criar e alterar cadastros"),
        CreatePermission("a0000000-0000-0000-0000-000000000303", "Cadastros.Delete", "Excluir cadastros"),
        CreatePermission("a0000000-0000-0000-0000-000000000304", "Tickets.Read", "Consultar tickets"),
        CreatePermission("a0000000-0000-0000-0000-000000000305", "Tickets.Checklist.Respond", "Responder checklist de tickets"),
        CreatePermission("a0000000-0000-0000-0000-000000000306", "Jobs.Sap.Schedule", "Agendar sincronização SAP"),
        CreatePermission("a0000000-0000-0000-0000-000000000307", "DirectoryIdentities.Resolve", "Resolver identidades no diretório")
    ];

    private static object[] CreateProfilePermissions() => PermissionIds
        .Select(permissionId => new { PerfilId = AdministracaoProfileId, PermissaoId = permissionId, CreationTime = SeededAt, CreatorUsername = SeedActorId, ModificationTime = SeededAt, ModifierUsername = SeedActorId })
        .Append(new { PerfilId = IntegracaoProfileId, PermissaoId = Guid.Parse("a0000000-0000-0000-0000-000000000301"), CreationTime = SeededAt, CreatorUsername = SeedActorId, ModificationTime = SeededAt, ModifierUsername = SeedActorId })
        .Append(new { PerfilId = IntegracaoProfileId, PermissaoId = Guid.Parse("a0000000-0000-0000-0000-000000000302"), CreationTime = SeededAt, CreatorUsername = SeedActorId, ModificationTime = SeededAt, ModifierUsername = SeedActorId })
        .Cast<object>()
        .ToArray();

    private static object CreatePermission(string id, string codigo, string descricao) =>
        new { Id = Guid.Parse(id), Codigo = codigo, Descricao = descricao, CreationTime = SeededAt, CreatorUsername = SeedActorId, ModificationTime = SeededAt, ModifierUsername = SeedActorId };

    private static readonly Guid[] PermissionIds =
    [
        Guid.Parse("a0000000-0000-0000-0000-000000000301"),
        Guid.Parse("a0000000-0000-0000-0000-000000000302"),
        Guid.Parse("a0000000-0000-0000-0000-000000000303"),
        Guid.Parse("a0000000-0000-0000-0000-000000000304"),
        Guid.Parse("a0000000-0000-0000-0000-000000000305"),
        Guid.Parse("a0000000-0000-0000-0000-000000000306"),
        Guid.Parse("a0000000-0000-0000-0000-000000000307")
    ];
}
