namespace Backend.App.Abstractions.Integrations;

public interface IIntegrationEventPublisher
{
    public Task PublishAsync<TEvent>(
        string eventId,
        TEvent integrationEvent,
        CancellationToken cancellationToken) where TEvent : class;
}
