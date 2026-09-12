namespace Backend.Infra.EventHub;

public sealed class EventHubOptions
{
    public const string SectionName = "EventHub";
    public string ConnectionString { get; init; } = string.Empty;
    public string CheckpointStorageConnectionString { get; init; } = string.Empty;
    public string CheckpointContainer { get; init; } = "eventhub-checkpoints";
    public string TicketsHub { get; init; } = "tickets";
    public string IntegracoesHub { get; init; } = "integracoes";
    public string ConsumerGroup { get; init; } = "$Default";
}
