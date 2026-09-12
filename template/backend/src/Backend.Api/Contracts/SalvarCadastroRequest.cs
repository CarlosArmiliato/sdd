using Backend.Domain.Cadastros;

namespace Backend.Api.Contracts;

public sealed record SalvarCadastroRequest(
    CadastroTipo Tipo,
    string Codigo,
    string Nome,
    string? CodigoExterno = null);
