using Backend.App.Abstractions.BackgroundJobs;
using Cortex.Mediator;

namespace Backend.Infra.Hangfire;

public sealed class CortexBackgroundCommandJob<TCommand>(IMediator mediator)
    where TCommand : class, IBackgroundCommand
{
    public Task ExecuteAsync(TCommand command, CancellationToken cancellationToken) =>
        mediator.SendAsync(command, cancellationToken);
}
