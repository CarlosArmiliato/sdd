using Backend.Domain.Auditing;

namespace Backend.Domain.Authorization;

public sealed class MapeamentoRolePerfil : AuditableEntity
{
    private MapeamentoRolePerfil() { }

    public MapeamentoRolePerfil(Guid id, string roleValue, Guid perfilId, RoleSubjectType subjectType, bool ativo = true)
    {
        Id = RequireId(id, nameof(id));
        RoleValue = RequireValue(roleValue, nameof(roleValue));
        PerfilId = RequireId(perfilId, nameof(perfilId));
        SubjectType = RequireSubjectType(subjectType);
        Ativo = ativo;
    }

    public Guid Id { get; private set; }
    public string RoleValue { get; private set; } = string.Empty;
    public Guid PerfilId { get; private set; }
    public RoleSubjectType SubjectType { get; private set; }
    public bool Ativo { get; private set; }
    public PerfilAcesso? Perfil { get; private set; }

    public bool AplicaPara(bool application) => Ativo && (SubjectType == RoleSubjectType.Both || application == (SubjectType == RoleSubjectType.Application));

    private static Guid RequireId(Guid value, string parameterName) =>
        value != Guid.Empty ? value : throw new ArgumentException("O identificador é obrigatório.", parameterName);

    private static RoleSubjectType RequireSubjectType(RoleSubjectType value) =>
        Enum.IsDefined(value) ? value : throw new ArgumentOutOfRangeException(nameof(value));

    private static string RequireValue(string value, string parameterName) =>
        !string.IsNullOrWhiteSpace(value) ? value.Trim() : throw new ArgumentException("O valor é obrigatório.", parameterName);
}
