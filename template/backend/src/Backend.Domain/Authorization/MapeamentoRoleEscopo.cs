using Backend.Domain.Auditing;

namespace Backend.Domain.Authorization;

public sealed class MapeamentoRoleEscopo : AuditableEntity
{
    public const string FilialScopeType = "Filial";

    private MapeamentoRoleEscopo() { }

    public MapeamentoRoleEscopo(Guid id, string roleValue, string scopeType, string scopeValue, bool ativo = true)
    {
        Id = RequireId(id, nameof(id));
        RoleValue = RequireValue(roleValue, nameof(roleValue));
        ScopeType = RequireScopeType(scopeType);
        ScopeValue = RequireValue(scopeValue, nameof(scopeValue));
        Ativo = ativo;
    }

    public Guid Id { get; private set; }
    public string RoleValue { get; private set; } = string.Empty;
    public string ScopeType { get; private set; } = string.Empty;
    public string ScopeValue { get; private set; } = string.Empty;
    public bool Ativo { get; private set; }

    private static Guid RequireId(Guid value, string parameterName) =>
        value != Guid.Empty ? value : throw new ArgumentException("O identificador é obrigatório.", parameterName);

    private static string RequireScopeType(string value)
    {
        string scopeType = RequireValue(value, nameof(value));
        return scopeType == FilialScopeType ? scopeType : throw new ArgumentException("O tipo de escopo não é permitido.", nameof(value));
    }

    private static string RequireValue(string value, string parameterName) =>
        !string.IsNullOrWhiteSpace(value) ? value.Trim() : throw new ArgumentException("O valor é obrigatório.", parameterName);
}
