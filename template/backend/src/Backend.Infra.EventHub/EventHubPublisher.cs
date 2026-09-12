using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Backend.App.Abstractions.Integrations;

namespace Backend.Infra.EventHub;

internal sealed class EventHubPublisher(EventHubProducerClient producer) : IIntegrationEventPublisher
{
    public async Task PublishAsync<TEvent>(string eventId, TEvent integrationEvent, CancellationToken cancellationToken)
        where TEvent : class
    {
        using EventDataBatch batch = await producer.CreateBatchAsync(cancellationToken);
        EventData eventData = new(JsonSerializer.SerializeToUtf8Bytes(integrationEvent));
        eventData.MessageId = eventId;
        eventData.ContentType = "application/json";
        if (!batch.TryAdd(eventData))
        {
            throw new InvalidOperationException("A mensagem excede o tamanho máximo aceito pelo Event Hub.");
        }
        await producer.SendAsync(batch, cancellationToken);
    }
}
