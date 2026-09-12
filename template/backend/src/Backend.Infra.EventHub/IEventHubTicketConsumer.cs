using Backend.Contracts.Events;

namespace Backend.Infra.EventHub;

public interface IEventHubTicketConsumer
{
    public Task RunAsync(
        Func<TicketRecebidoV1, CancellationToken, Task> handler,
        CancellationToken cancellationToken);
}
