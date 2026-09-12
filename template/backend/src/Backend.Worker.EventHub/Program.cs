using Backend.App;
using Backend.App.Abstractions;
using Backend.Infra.EventHub;
using Backend.Infra.Postgres;
using Backend.App.Time;
using Backend.Worker.EventHub;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
string postgres = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("ConnectionStrings:Postgres não configurada.");
builder.Services.AddApp(typeof(WorkerAssemblyMarker));
builder.Services.AddPostgres(postgres);
builder.Services.AddOptions<EventHubOptions>()
    .Bind(builder.Configuration.GetSection(EventHubOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), "EventHub:ConnectionString é obrigatória.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.CheckpointStorageConnectionString), "EventHub:CheckpointStorageConnectionString é obrigatória.")
    .ValidateOnStart();
builder.Services.AddTicketEventHubConsumer();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddHostedService<TicketEventWorker>();

await builder.Build().RunAsync();
