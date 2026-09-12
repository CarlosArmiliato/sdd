using Backend.App.Abstractions.BackgroundJobs;
using Hangfire;

namespace Backend.Infra.Hangfire;

internal sealed class HangfireBackgroundCommandScheduler(IBackgroundJobClient client)
    : IBackgroundCommandScheduler
{
    public string Enqueue<TCommand>(TCommand command) where TCommand : class, IBackgroundCommand =>
        client.Enqueue<CortexBackgroundCommandJob<TCommand>>(job =>
            job.ExecuteAsync(command, CancellationToken.None));
}
