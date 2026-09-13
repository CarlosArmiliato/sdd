using Backend.Domain.Auditing;

namespace Backend.Domain.Authorization;

public sealed class PerfilAcesso : AuditableEntity
{
    private PerfilAcesso() { }

    public PerfilAcesso(Guid id, string codigo, string nome, bool ativo = true)
    {
        Id = RequireId(id, nameof(id));
        Codigo = RequireValue(codigo, nameof(codigo));
        Nome = RequireValue(nome, nameof(nome));
        Ativo = ativo;
    }

    public Guid Id { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Nome { get; private set; } = string.Empty;
    public bool Ativo { get; private set; }

    public void DefinirAtivo(bool ativo) => Ativo = ativo;

    private static Guid RequireId(Guid value, string parameterName) =>
        value != Guid.Empty ? value : throw new ArgumentException("O identificador é obrigatório.", parameterName);

    private static string RequireValue(string value, string parameterName) =>
        !string.IsNullOrWhiteSpace(value) ? value.Trim() : throw new ArgumentException("O valor é obrigatório.", parameterName);
}
