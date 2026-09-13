using Backend.Domain.Auditing;

namespace Backend.Domain.Authorization;

public sealed class PerfilPermissao : AuditableEntity
{
    private PerfilPermissao() { }

    public PerfilPermissao(Guid perfilId, Guid permissaoId)
    {
        PerfilId = RequireId(perfilId, nameof(perfilId));
        PermissaoId = RequireId(permissaoId, nameof(permissaoId));
    }

    public Guid PerfilId { get; private set; }
    public Guid PermissaoId { get; private set; }
    public PerfilAcesso? Perfil { get; private set; }
    public Permissao? Permissao { get; private set; }

    private static Guid RequireId(Guid value, string parameterName) =>
        value != Guid.Empty ? value : throw new ArgumentException("O identificador é obrigatório.", parameterName);
}
