using Backend.App.Abstractions.Persistence;
using Backend.Domain.Integracoes;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infra.Postgres.Persistence;

internal sealed class IntegracaoRepository(BackendDbContext dbContext) : IIntegracaoRepository
{
    public async Task<IReadOnlyCollection<Integracao>> BuscarEIniciarPendentesAsync(
        int limite,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        List<Integracao> integracoes = await dbContext.Integracoes
            .FromSqlInterpolated($"""
                SELECT * FROM "Integracoes"
                WHERE ("Status" = 'Pendente' AND "DisponivelEm" <= now())
                   OR ("Status" = 'Processando' AND "ProcessamentoIniciadoEm" < now() - interval '5 minutes')
                ORDER BY "CriadaEm"
                FOR UPDATE SKIP LOCKED
                LIMIT {limite}
                """)
            .ToListAsync(cancellationToken);
        foreach (Integracao integracao in integracoes)
        {
            if (integracao.Status == IntegracaoStatus.Processando)
            {
                integracao.RegistrarFalha("Lease de processamento expirado.", DateTimeOffset.UtcNow);
            }
            integracao.IniciarProcessamento(DateTimeOffset.UtcNow);
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return integracoes;
    }

    public Task<Integracao?> ObterAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Integracoes.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task SalvarAsync(Integracao integracao, CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
