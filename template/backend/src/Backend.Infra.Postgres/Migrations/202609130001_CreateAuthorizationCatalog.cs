using Backend.Infra.Postgres.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#pragma warning disable CA1861

namespace Backend.Infra.Postgres.Migrations;

[DbContext(typeof(BackendDbContext))]
[Migration("202609130001_CreateAuthorizationCatalog")]
public sealed class CreateAuthorizationCatalog : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        CreateProfiles(migrationBuilder);
        CreatePermissions(migrationBuilder);
        CreateRoleMappings(migrationBuilder);
        CreateApplicationIdentities(migrationBuilder);
        SeedCatalog(migrationBuilder);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "MapeamentosRoleEscopo");
        migrationBuilder.DropTable(name: "MapeamentosRolePerfil");
        migrationBuilder.DropTable(name: "PerfisPermissao");
        migrationBuilder.DropTable(name: "IdentidadesAplicacao");
        migrationBuilder.DropTable(name: "Permissoes");
        migrationBuilder.DropTable(name: "PerfisAcesso");
    }

    private static void CreateProfiles(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PerfisAcesso",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Codigo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                Nome = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                Ativo = table.Column<bool>(type: "boolean", nullable: false),
                CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                CreatorUsername = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false, defaultValue: "v1:system:authorization-seed"),
                ModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                ModifierUsername = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false, defaultValue: "v1:system:authorization-seed")
            },
            constraints: table => table.PrimaryKey("PK_PerfisAcesso", x => x.Id));
        migrationBuilder.CreateIndex(name: "IX_PerfisAcesso_Codigo", table: "PerfisAcesso", column: "Codigo", unique: true);
    }

    private static void CreatePermissions(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Permissoes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Codigo = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                Descricao = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                CreatorUsername = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false, defaultValue: "v1:system:authorization-seed"),
                ModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                ModifierUsername = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false, defaultValue: "v1:system:authorization-seed")
            },
            constraints: table => table.PrimaryKey("PK_Permissoes", x => x.Id));
        migrationBuilder.CreateIndex(name: "IX_Permissoes_Codigo", table: "Permissoes", column: "Codigo", unique: true);
        migrationBuilder.CreateTable(
            name: "PerfisPermissao",
            columns: table => new
            {
                PerfilId = table.Column<Guid>(type: "uuid", nullable: false),
                PermissaoId = table.Column<Guid>(type: "uuid", nullable: false),
                CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                CreatorUsername = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false, defaultValue: "v1:system:authorization-seed"),
                ModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                ModifierUsername = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false, defaultValue: "v1:system:authorization-seed")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PerfisPermissao", x => new { x.PerfilId, x.PermissaoId });
                table.ForeignKey("FK_PerfisPermissao_PerfisAcesso_PerfilId", x => x.PerfilId, "PerfisAcesso", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_PerfisPermissao_Permissoes_PermissaoId", x => x.PermissaoId, "Permissoes", "Id", onDelete: ReferentialAction.Restrict);
            });
        migrationBuilder.CreateIndex(name: "IX_PerfisPermissao_PermissaoId", table: "PerfisPermissao", column: "PermissaoId");
    }

    private static void CreateRoleMappings(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "MapeamentosRolePerfil",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                RoleValue = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                PerfilId = table.Column<Guid>(type: "uuid", nullable: false),
                SubjectType = table.Column<int>(type: "integer", nullable: false, comment: "1 = User — Usuário; 2 = Application — Aplicação; 3 = Both — Usuário ou aplicação"),
                Ativo = table.Column<bool>(type: "boolean", nullable: false),
                CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                CreatorUsername = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false, defaultValue: "v1:system:authorization-seed"),
                ModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                ModifierUsername = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false, defaultValue: "v1:system:authorization-seed")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MapeamentosRolePerfil", x => x.Id);
                table.ForeignKey("FK_MapeamentosRolePerfil_PerfisAcesso_PerfilId", x => x.PerfilId, "PerfisAcesso", "Id", onDelete: ReferentialAction.Restrict);
                table.CheckConstraint("CK_MapeamentosRolePerfil_SubjectType", "\"SubjectType\" IN (1, 2, 3)");
            });
        migrationBuilder.CreateIndex(name: "IX_MapeamentosRolePerfil_RoleValue_PerfilId_SubjectType", table: "MapeamentosRolePerfil", columns: new[] { "RoleValue", "PerfilId", "SubjectType" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_MapeamentosRolePerfil_PerfilId", table: "MapeamentosRolePerfil", column: "PerfilId");
        migrationBuilder.CreateTable(
            name: "MapeamentosRoleEscopo",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                RoleValue = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                ScopeType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                ScopeValue = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                Ativo = table.Column<bool>(type: "boolean", nullable: false),
                CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                CreatorUsername = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false, defaultValue: "v1:system:authorization-seed"),
                ModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                ModifierUsername = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false, defaultValue: "v1:system:authorization-seed")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MapeamentosRoleEscopo", x => x.Id);
                table.CheckConstraint("CK_MapeamentosRoleEscopo_ScopeType", "\"ScopeType\" = 'Filial'");
            });
        migrationBuilder.CreateIndex(name: "IX_MapeamentosRoleEscopo_RoleValue_ScopeType_ScopeValue", table: "MapeamentosRoleEscopo", columns: new[] { "RoleValue", "ScopeType", "ScopeValue" }, unique: true);
    }

    private static void CreateApplicationIdentities(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "IdentidadesAplicacao",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                ObjectId = table.Column<Guid>(type: "uuid", nullable: false),
                ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                Tipo = table.Column<int>(type: "integer", nullable: false, comment: "1 = Interna — Aplicação interna; 2 = Fornecedor — Fornecedor"),
                NomeTecnico = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                FornecedorCodigo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                Ativa = table.Column<bool>(type: "boolean", nullable: false),
                CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                CreatorUsername = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false, defaultValue: "v1:system:authorization-seed"),
                ModificationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                ModifierUsername = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false, defaultValue: "v1:system:authorization-seed")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_IdentidadesAplicacao", x => x.Id);
                table.CheckConstraint("CK_IdentidadesAplicacao_Fornecedor", "(\"Tipo\" = 1 AND \"FornecedorCodigo\" IS NULL) OR (\"Tipo\" = 2 AND \"FornecedorCodigo\" IS NOT NULL)");
            });
        migrationBuilder.CreateIndex(name: "IX_IdentidadesAplicacao_TenantId_ObjectId", table: "IdentidadesAplicacao", columns: new[] { "TenantId", "ObjectId" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_IdentidadesAplicacao_TenantId_ClientId", table: "IdentidadesAplicacao", columns: new[] { "TenantId", "ClientId" }, unique: true);
    }

    private static void SeedCatalog(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData("PerfisAcesso", new[] { "Id", "Codigo", "Nome", "Ativo" }, new object[,]
        {
            { Guid.Parse("a0000000-0000-0000-0000-000000000001"), "Administracao", "Administração do sistema", true },
            { Guid.Parse("a0000000-0000-0000-0000-000000000002"), "IntegracaoCadastrosEscrita", "Integração de cadastros com escrita", true }
        });
        migrationBuilder.InsertData("Permissoes", new[] { "Id", "Codigo", "Descricao" }, new object[,]
        {
            { PermissionId(1), "Cadastros.Read", "Consultar cadastros" }, { PermissionId(2), "Cadastros.Write", "Criar e alterar cadastros" },
            { PermissionId(3), "Cadastros.Delete", "Excluir cadastros" }, { PermissionId(4), "Tickets.Read", "Consultar tickets" },
            { PermissionId(5), "Tickets.Checklist.Respond", "Responder checklist de tickets" }, { PermissionId(6), "Jobs.Sap.Schedule", "Agendar sincronização SAP" },
            { PermissionId(7), "DirectoryIdentities.Resolve", "Resolver identidades no diretório" }
        });
        Guid administration = Guid.Parse("a0000000-0000-0000-0000-000000000001");
        migrationBuilder.InsertData("PerfisPermissao", new[] { "PerfilId", "PermissaoId" }, ProfilePermissions(administration));
        migrationBuilder.InsertData("PerfisPermissao", new[] { "PerfilId", "PermissaoId" }, new object[,] { { Guid.Parse("a0000000-0000-0000-0000-000000000002"), PermissionId(1) }, { Guid.Parse("a0000000-0000-0000-0000-000000000002"), PermissionId(2) } });
        migrationBuilder.InsertData("MapeamentosRolePerfil", new[] { "Id", "RoleValue", "PerfilId", "SubjectType", "Ativo" }, new object[,] { { Guid.Parse("a0000000-0000-0000-0000-000000000101"), "Administracao", administration, 3, true }, { Guid.Parse("a0000000-0000-0000-0000-000000000102"), "Cadastros.Integration.Write", Guid.Parse("a0000000-0000-0000-0000-000000000002"), 2, true } });
        migrationBuilder.InsertData("MapeamentosRoleEscopo", new[] { "Id", "RoleValue", "ScopeType", "ScopeValue", "Ativo" }, new object[,] { { Guid.Parse("a0000000-0000-0000-0000-000000000201"), "Scope.Branch.FilialA", "Filial", "FilialA", true } });
    }

    private static Guid PermissionId(int value) => Guid.Parse($"a0000000-0000-0000-0000-{value + 300:000000000000}");

    private static object[,] ProfilePermissions(Guid profileId) => new object[,]
    {
        { profileId, PermissionId(1) }, { profileId, PermissionId(2) }, { profileId, PermissionId(3) }, { profileId, PermissionId(4) },
        { profileId, PermissionId(5) }, { profileId, PermissionId(6) }, { profileId, PermissionId(7) }
    };
}

#pragma warning restore CA1861
