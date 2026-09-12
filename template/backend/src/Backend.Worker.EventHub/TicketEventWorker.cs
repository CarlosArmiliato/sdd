using Backend.App.Tickets;
using Backend.Infra.EventHub;
using Backend.Contracts.Events;
using Cortex.Mediator;

namespace Backend.Worker.EventHub;

public sealed partial class TicketEventWorker(
    IEventHubTicketConsumer consumer,
    IServiceScopeFactory scopeFactory,
    ILogger<TicketEventWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await consumer.RunAsync(async (message, cancellationToken) =>
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.SendAsync(new ReceberTicketCommand(message), cancellationToken);
            }
            catch (Exception exception)
            {
                LogConsumptionFailure(logger, exception, message.EventId);
                throw;
            }
        }, stoppingToken);
    }

    [LoggerMessage(EventId = 1001, Level = LogLevel.Error, Message = "Falha ao consumir evento {EventId}.")]
    private static partial void LogConsumptionFailure(ILogger logger, Exception exception, string eventId);
}
