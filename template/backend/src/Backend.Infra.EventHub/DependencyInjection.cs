using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Azure.Storage.Blobs;
using Backend.App.Abstractions.Integrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Backend.Infra.EventHub;

public static class DependencyInjection
{
    public static IServiceCollection AddEventHubPublisher(this IServiceCollection services)
    {
        services.AddSingleton(provider =>
        {
            EventHubOptions options = provider.GetRequiredService<IOptions<EventHubOptions>>().Value;
            return new EventHubProducerClient(options.ConnectionString, options.IntegracoesHub);
        });
        services.AddSingleton<IIntegrationEventPublisher, EventHubPublisher>();
        return services;
    }

    public static IServiceCollection AddTicketEventHubConsumer(this IServiceCollection services)
    {
        services.AddSingleton(provider =>
        {
            EventHubOptions options = provider.GetRequiredService<IOptions<EventHubOptions>>().Value;
            return new BlobContainerClient(options.CheckpointStorageConnectionString, options.CheckpointContainer);
        });
        services.AddSingleton(provider =>
        {
            EventHubOptions options = provider.GetRequiredService<IOptions<EventHubOptions>>().Value;
            BlobContainerClient checkpoints = provider.GetRequiredService<BlobContainerClient>();
            return new EventProcessorClient(checkpoints, options.ConsumerGroup, options.ConnectionString, options.TicketsHub);
        });
        services.AddSingleton<IEventHubTicketConsumer, EventHubTicketConsumer>();
        return services;
    }
}
