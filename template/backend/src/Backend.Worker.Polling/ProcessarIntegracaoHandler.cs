using Backend.App.Abstractions;
using Backend.App.Abstractions.Integrations;
using Backend.App.Abstractions.Persistence;
using Backend.App.Integracoes;
using Backend.Domain.Integracoes;
using Cortex.Mediator.Commands;

namespace Backend.Worker.Polling;

public sealed class ProcessarIntegracaoHandler(
    IIntegracaoRepository repository,
    IEnumerable<IIntegracaoHandler> handlers,
    IClock clock) : ICommandHandler<ProcessarIntegracaoCommand>
{
    public async Task Handle(ProcessarIntegracaoCommand command, CancellationToken cancellationToken)
    {
        Integracao integracao = await repository.ObterAsync(command.IntegracaoId, cancellationToken)
            ?? throw new KeyNotFoundException("Integração não encontrada.");
        if (integracao.Status == IntegracaoStatus.Concluida || integracao.Status == IntegracaoStatus.FalhaDefinitiva)
        {
            return;
        }
        if (integracao.Status != IntegracaoStatus.Processando)
        {
            throw new InvalidOperationException("A integração precisa ser reservada antes do processamento.");
        }
        try
        {
            IIntegracaoHandler handler = handlers.SingleOrDefault(x => x.Tipo == integracao.Tipo)
                ?? throw new InvalidOperationException($"Handler não registrado para {integracao.Tipo}.");
            await handler.HandleAsync(integracao, cancellationToken);
            integracao.Concluir(clock.UtcNow);
            await repository.SalvarAsync(integracao, cancellationToken);
        }
        catch (Exception exception)
        {
            TimeSpan espera = TimeSpan.FromMinutes(Math.Pow(2, integracao.Tentativas));
            integracao.RegistrarFalha(exception.GetType().Name, clock.UtcNow.Add(espera));
            await repository.SalvarAsync(integracao, cancellationToken);
            throw;
        }
    }
}
