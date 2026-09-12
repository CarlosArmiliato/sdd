using Cortex.Mediator.Commands;

namespace Backend.App.Abstractions.BackgroundJobs;

public interface IBackgroundCommand : ICommand;

public interface IBackgroundCommandScheduler
{
    public string Enqueue<TCommand>(TCommand command) where TCommand : class, IBackgroundCommand;
}
