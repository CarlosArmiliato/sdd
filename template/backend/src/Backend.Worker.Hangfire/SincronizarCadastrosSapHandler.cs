using Backend.App.Abstractions.Integrations;
using Backend.App.Abstractions.Persistence;
using Backend.App.BackgroundJobs;
using Backend.Domain.Cadastros;
using Cortex.Mediator.Commands;

namespace Backend.Worker.Hangfire;

public sealed class SincronizarCadastrosSapHandler(
    ISapCadastroSource sap,
    ICadastroRepository repository) : ICommandHandler<SincronizarCadastrosSapCommand>
{
    public async Task Handle(SincronizarCadastrosSapCommand command, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<SapCadastro> cadastros = await sap.BuscarCadastrosAsync(cancellationToken);
        foreach (SapCadastro item in cadastros)
        {
            Cadastro? cadastro = await repository.ObterPorCodigoExternoAsync(item.Tipo, item.CodigoExterno, cancellationToken);
            if (cadastro is null)
            {
                cadastro = new Cadastro(item.Tipo, item.Codigo, item.Nome);
                cadastro.Atualizar(item.Codigo, item.Nome, item.CodigoExterno);
            }
            else
            {
                cadastro.Atualizar(item.Codigo, item.Nome);
            }
            await repository.SalvarAsync(cadastro, cancellationToken);
        }
    }
}
