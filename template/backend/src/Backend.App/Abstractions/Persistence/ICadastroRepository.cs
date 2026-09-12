using Backend.Domain.Cadastros;

namespace Backend.App.Abstractions.Persistence;

public interface ICadastroRepository
{
    public Task<IReadOnlyCollection<Cadastro>> ListarAsync(CadastroTipo tipo, CancellationToken cancellationToken);
    public Task<Cadastro?> ObterAsync(Guid id, CancellationToken cancellationToken);
    public Task<Cadastro?> ObterPorCodigoExternoAsync(CadastroTipo tipo, string codigoExterno, CancellationToken cancellationToken);
    public Task SalvarAsync(Cadastro cadastro, CancellationToken cancellationToken);
    public Task ExcluirAsync(Cadastro cadastro, CancellationToken cancellationToken);
}
