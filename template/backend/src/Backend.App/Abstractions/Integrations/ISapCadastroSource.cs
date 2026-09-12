using Backend.Domain.Cadastros;

namespace Backend.App.Abstractions.Integrations;

public sealed record SapCadastro(
    CadastroTipo Tipo,
    string Codigo,
    string Nome,
    string CodigoExterno);

public interface ISapCadastroSource
{
    public Task<IReadOnlyCollection<SapCadastro>> BuscarCadastrosAsync(CancellationToken cancellationToken);
}
