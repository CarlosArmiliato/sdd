using Backend.Domain.Auditing;

namespace Backend.Domain.Authorization;

public sealed class Permissao : AuditableEntity
{
    private Permissao() { }

    public Permissao(Guid id, string codigo, string descricao)
    {
        Id = RequireId(id, nameof(id));
        Codigo = RequirePermissionCode(codigo);
        Descricao = RequireValue(descricao, nameof(descricao));
    }

    public Guid Id { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;

    private static Guid RequireId(Guid value, string parameterName) =>
        value != Guid.Empty ? value : throw new ArgumentException("O identificador é obrigatório.", parameterName);

    private static string RequirePermissionCode(string value)
    {
        string code = RequireValue(value, nameof(value));
        return code.Contains('.', StringComparison.Ordinal) ? code : throw new ArgumentException("O código deve conter recurso e ação.", nameof(value));
    }

    private static string RequireValue(string value, string parameterName) =>
        !string.IsNullOrWhiteSpace(value) ? value.Trim() : throw new ArgumentException("O valor é obrigatório.", parameterName);
}
