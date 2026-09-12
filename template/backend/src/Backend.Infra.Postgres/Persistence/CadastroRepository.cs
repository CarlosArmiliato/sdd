using Backend.App.Abstractions.Persistence;
using Backend.Domain.Cadastros;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infra.Postgres.Persistence;

internal sealed class CadastroRepository(BackendDbContext dbContext) : ICadastroRepository
{
    public async Task<IReadOnlyCollection<Cadastro>> ListarAsync(CadastroTipo tipo, CancellationToken cancellationToken) =>
        await dbContext.Cadastros.Where(x => x.Tipo == tipo).OrderBy(x => x.Nome).ToListAsync(cancellationToken);

    public Task<Cadastro?> ObterAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Cadastros.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Cadastro?> ObterPorCodigoExternoAsync(CadastroTipo tipo, string codigoExterno, CancellationToken cancellationToken) =>
        dbContext.Cadastros.SingleOrDefaultAsync(x => x.Tipo == tipo && x.CodigoExterno == codigoExterno, cancellationToken);

    public async Task SalvarAsync(Cadastro cadastro, CancellationToken cancellationToken)
    {
        if (dbContext.Entry(cadastro).State == EntityState.Detached)
        {
            dbContext.Cadastros.Add(cadastro);
        }
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ExcluirAsync(Cadastro cadastro, CancellationToken cancellationToken)
    {
        dbContext.Cadastros.Remove(cadastro);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
