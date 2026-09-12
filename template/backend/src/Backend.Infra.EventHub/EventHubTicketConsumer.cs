using Azure.Messaging.EventHubs;
using Azure.Storage.Blobs;
using Backend.Contracts.Events;

namespace Backend.Infra.EventHub;

internal sealed partial class EventHubTicketConsumer(
    EventProcessorClient processor,
    BlobContainerClient checkpointContainer,
    Microsoft.Extensions.Logging.ILogger<EventHubTicketConsumer> logger) : IEventHubTicketConsumer
{
    public async Task RunAsync(
        Func<TicketRecebidoV1, CancellationToken, Task> handler,
        CancellationToken cancellationToken)
    {
        await checkpointContainer.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        processor.ProcessEventAsync += async args =>
        {
            TicketRecebidoV1? message = args.Data.EventBody.ToObjectFromJson<TicketRecebidoV1>();
            if (message is null)
            {
                return;
            }

            await handler(message, cancellationToken);
            await args.UpdateCheckpointAsync(cancellationToken);
        };
        processor.ProcessErrorAsync += args =>
        {
            LogProcessorFailure(logger, args.Exception, args.PartitionId);
            return Task.CompletedTask;
        };

        await processor.StartProcessingAsync(cancellationToken);
        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        finally
        {
            await processor.StopProcessingAsync(CancellationToken.None);
        }
    }

    [Microsoft.Extensions.Logging.LoggerMessage(
        EventId = 3001,
        Level = Microsoft.Extensions.Logging.LogLevel.Error,
        Message = "Falha no EventProcessor da partição {PartitionId}.")]
    private static partial void LogProcessorFailure(
        Microsoft.Extensions.Logging.ILogger logger,
        Exception exception,
        string partitionId);
}
