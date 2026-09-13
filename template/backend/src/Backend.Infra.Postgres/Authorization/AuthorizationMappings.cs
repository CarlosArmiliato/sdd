using Backend.Domain.Authorization;
using Backend.Infra.Postgres.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infra.Postgres.Authorization;

internal static class AuthorizationMappings
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        ConfigureProfiles(modelBuilder);
        ConfigurePermissions(modelBuilder);
        ConfigureRoleMappings(modelBuilder);
        ConfigureApplicationIdentities(modelBuilder);
        AuthorizationCatalogSeed.Apply(modelBuilder);
    }

    private static void ConfigureProfiles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PerfilAcesso>(entity =>
        {
            entity.ToTable("PerfisAcesso");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Codigo).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Nome).HasMaxLength(160).IsRequired();
            entity.HasIndex(x => x.Codigo).IsUnique();
        });
        modelBuilder.Entity<PerfilPermissao>(entity =>
        {
            entity.ToTable("PerfisPermissao");
            entity.HasKey(x => new { x.PerfilId, x.PermissaoId });
            entity.HasOne(x => x.Perfil).WithMany().HasForeignKey(x => x.PerfilId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Permissao).WithMany().HasForeignKey(x => x.PermissaoId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurePermissions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Permissao>(entity =>
        {
            entity.ToTable("Permissoes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Codigo).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Descricao).HasMaxLength(240).IsRequired();
            entity.HasIndex(x => x.Codigo).IsUnique();
        });
    }

    private static void ConfigureRoleMappings(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MapeamentoRolePerfil>(entity =>
        {
            entity.ToTable("MapeamentosRolePerfil", table => table.HasCheckConstraint("CK_MapeamentosRolePerfil_SubjectType", "\"SubjectType\" IN (1, 2, 3)"));
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RoleValue).HasMaxLength(120).IsRequired();
            entity.Property(x => x.SubjectType).HasEnumComment();
            entity.HasIndex(x => new { x.RoleValue, x.PerfilId, x.SubjectType }).IsUnique();
            entity.HasOne(x => x.Perfil).WithMany().HasForeignKey(x => x.PerfilId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<MapeamentoRoleEscopo>(entity =>
        {
            entity.ToTable("MapeamentosRoleEscopo", table => table.HasCheckConstraint("CK_MapeamentosRoleEscopo_ScopeType", "\"ScopeType\" = 'Filial'"));
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RoleValue).HasMaxLength(120).IsRequired();
            entity.Property(x => x.ScopeType).HasMaxLength(80).IsRequired();
            entity.Property(x => x.ScopeValue).HasMaxLength(120).IsRequired();
            entity.HasIndex(x => new { x.RoleValue, x.ScopeType, x.ScopeValue }).IsUnique();
        });
    }

    private static void ConfigureApplicationIdentities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdentidadeAplicacao>(entity =>
        {
            entity.ToTable("IdentidadesAplicacao", table =>
            {
                table.HasCheckConstraint("CK_IdentidadesAplicacao_Fornecedor", "(\"Tipo\" = 1 AND \"FornecedorCodigo\" IS NULL) OR (\"Tipo\" = 2 AND \"FornecedorCodigo\" IS NOT NULL)");
            });
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Tipo).HasEnumComment();
            entity.Property(x => x.NomeTecnico).HasMaxLength(160).IsRequired();
            entity.Property(x => x.FornecedorCodigo).HasMaxLength(80);
            entity.HasIndex(x => new { x.TenantId, x.ObjectId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ClientId }).IsUnique();
        });
    }
}
