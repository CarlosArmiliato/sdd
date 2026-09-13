using Backend.Domain.Auditing;

namespace Backend.Domain.Authorization;

public sealed class IdentidadeAplicacao : AuditableEntity
{
    private IdentidadeAplicacao() { }

    public IdentidadeAplicacao(
        Guid id,
        Guid tenantId,
        Guid objectId,
        Guid clientId,
        IdentidadeAplicacaoTipo tipo,
        string nomeTecnico,
        string? fornecedorCodigo,
        bool ativa = true)
    {
        Id = RequireId(id, nameof(id));
        TenantId = RequireId(tenantId, nameof(tenantId));
        ObjectId = RequireId(objectId, nameof(objectId));
        ClientId = RequireId(clientId, nameof(clientId));
        Tipo = RequireType(tipo);
        NomeTecnico = RequireValue(nomeTecnico, nameof(nomeTecnico));
        FornecedorCodigo = RequireSupplierCode(tipo, fornecedorCodigo);
        Ativa = ativa;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid ObjectId { get; private set; }
    public Guid ClientId { get; private set; }
    public IdentidadeAplicacaoTipo Tipo { get; private set; }
    public string NomeTecnico { get; private set; } = string.Empty;
    public string? FornecedorCodigo { get; private set; }
    public bool Ativa { get; private set; }

    public bool CorrespondeA(Guid tenantId, Guid objectId, Guid clientId) =>
        Ativa && TenantId == tenantId && ObjectId == objectId && ClientId == clientId;

    public void DefinirAtiva(bool ativa) => Ativa = ativa;

    private static Guid RequireId(Guid value, string parameterName) =>
        value != Guid.Empty ? value : throw new ArgumentException("O identificador é obrigatório.", parameterName);

    private static IdentidadeAplicacaoTipo RequireType(IdentidadeAplicacaoTipo value) =>
        Enum.IsDefined(value) ? value : throw new ArgumentOutOfRangeException(nameof(value));

    private static string? RequireSupplierCode(IdentidadeAplicacaoTipo tipo, string? fornecedorCodigo)
    {
        if (tipo == IdentidadeAplicacaoTipo.Fornecedor)
        {
            return RequireValue(fornecedorCodigo ?? string.Empty, nameof(fornecedorCodigo));
        }
        return string.IsNullOrWhiteSpace(fornecedorCodigo) ? null : throw new ArgumentException("Aplicações internas não possuem código de fornecedor.", nameof(fornecedorCodigo));
    }

    private static string RequireValue(string value, string parameterName) =>
        !string.IsNullOrWhiteSpace(value) ? value.Trim() : throw new ArgumentException("O valor é obrigatório.", parameterName);
}
