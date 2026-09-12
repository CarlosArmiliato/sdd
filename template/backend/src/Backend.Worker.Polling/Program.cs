using Backend.App;
using Backend.App.Abstractions;
using Backend.Infra.EventHub;
using Backend.Infra.Gate.EmpresaA;
using Backend.Infra.Gate.EmpresaB;
using Backend.Infra.Postgres;
using Backend.App.Time;
using Backend.Worker.Polling;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
string postgres = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("ConnectionStrings:Postgres não configurada.");
builder.Services.AddApp(typeof(WorkerAssemblyMarker));
builder.Services.AddPostgres(postgres);
builder.Services.AddOptions<EventHubOptions>()
    .Bind(builder.Configuration.GetSection(EventHubOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), "EventHub:ConnectionString é obrigatória.")
    .ValidateOnStart();
builder.Services.AddEventHubPublisher();
builder.Services.AddEmpresaAGate();
builder.Services.AddEmpresaBGate();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddHostedService<IntegracoesPollingWorker>();

await builder.Build().RunAsync();
