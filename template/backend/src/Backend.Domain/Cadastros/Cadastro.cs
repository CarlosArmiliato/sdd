using Backend.Domain.Auditing;

namespace Backend.Domain.Cadastros;

public enum CadastroTipo
{
    Fazenda,
    AnoAgricola,
    Safra,
    Cultura
}

public sealed class Cadastro : AuditableEntity
{
    private Cadastro() { }

    public Cadastro(CadastroTipo tipo, string codigo, string nome)
    {
        Id = Guid.NewGuid();
        Tipo = tipo;
        Atualizar(codigo, nome);
    }

    public Guid Id { get; private set; }
    public CadastroTipo Tipo { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Nome { get; private set; } = string.Empty;
    public string? CodigoExterno { get; private set; }

    public void Atualizar(string codigo, string nome, string? codigoExterno = null)
    {
        Codigo = codigo.Trim();
        Nome = nome.Trim();
        CodigoExterno = codigoExterno;
    }
}
