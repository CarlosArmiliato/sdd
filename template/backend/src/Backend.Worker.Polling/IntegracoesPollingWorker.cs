using Backend.App.Abstractions.Persistence;
using Backend.App.Integracoes;
using Backend.Domain.Integracoes;
using Cortex.Mediator;

namespace Backend.Worker.Polling;

public sealed partial class IntegracoesPollingWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<IntegracoesPollingWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            IIntegracaoRepository repository = scope.ServiceProvider.GetRequiredService<IIntegracaoRepository>();
            IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            IReadOnlyCollection<Integracao> pendentes = await repository.BuscarEIniciarPendentesAsync(50, stoppingToken);
            foreach (Integracao integracao in pendentes)
            {
                try
                {
                    await mediator.SendAsync(new ProcessarIntegracaoCommand(integracao.Id), stoppingToken);
                }
                catch (Exception exception)
                {
                    LogProcessingFailure(logger, exception, integracao.Id);
                }
            }
            await Task.Delay(pendentes.Count == 0 ? TimeSpan.FromSeconds(15) : TimeSpan.FromSeconds(1), stoppingToken);
        }
    }

    [LoggerMessage(EventId = 2001, Level = LogLevel.Error, Message = "Falha ao processar integração {IntegracaoId}.")]
    private static partial void LogProcessingFailure(ILogger logger, Exception exception, Guid integracaoId);
}
