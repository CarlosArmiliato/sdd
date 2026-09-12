namespace Backend.Domain.Cadastros;

public sealed class Cadastro : Backend.Domain.Auditing.AuditableEntity
{
    public Guid Id { get; private set; }
    public CadastroTipo Tipo { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Nome { get; private set; } = string.Empty;
    public string? CodigoExterno { get; private set; }

    private Cadastro() { }

    public Cadastro(CadastroTipo tipo, string codigo, string nome)
    {
        Id = Guid.NewGuid();
        Tipo = tipo;
        Atualizar(codigo, nome);
    }

    public void Atualizar(string codigo, string nome, string? codigoExterno = null)
    {
        Codigo = codigo.Trim();
        Nome = nome.Trim();
        CodigoExterno = codigoExterno;
    }
}
