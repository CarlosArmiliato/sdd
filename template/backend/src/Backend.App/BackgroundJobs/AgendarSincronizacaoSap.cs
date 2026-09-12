using Backend.App.Abstractions.BackgroundJobs;
using Cortex.Mediator.Commands;

namespace Backend.App.BackgroundJobs;

public sealed record AgendarSincronizacaoSapCommand : ICommand<string>;

public sealed class AgendarSincronizacaoSapHandler(IBackgroundCommandScheduler scheduler)
    : ICommandHandler<AgendarSincronizacaoSapCommand, string>
{
    public Task<string> Handle(
        AgendarSincronizacaoSapCommand command,
        CancellationToken cancellationToken)
    {
        string jobId = scheduler.Enqueue(new SincronizarCadastrosSapCommand());
        return Task.FromResult(jobId);
    }
}
