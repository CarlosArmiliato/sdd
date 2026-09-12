using Backend.App;
using Backend.App.BackgroundJobs;
using Backend.Infra.Gate.Sap;
using Backend.Infra.Hangfire;
using Backend.Infra.Postgres;
using Backend.Worker.Hangfire;
using Hangfire;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
string postgres = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("ConnectionStrings:Postgres não configurada.");
builder.Services.AddApp(typeof(WorkerAssemblyMarker));
builder.Services.AddPostgres(postgres);
builder.Services.AddOptions<SapOptions>()
    .Bind(builder.Configuration.GetSection(SapOptions.SectionName))
    .Validate(options => options.BaseAddress is not null, "Sap:BaseAddress é obrigatória.")
    .ValidateOnStart();
builder.Services.AddSapGate();
builder.Services.AddHangfireWorker(postgres);

IHost host = builder.Build();
using (IServiceScope scope = host.Services.CreateScope())
{
    IRecurringJobManager recurringJobs = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    recurringJobs.AddOrUpdate<CortexBackgroundCommandJob<SincronizarCadastrosSapCommand>>(
        "sincronizacao-cadastros-sap-diaria",
        job => job.ExecuteAsync(new SincronizarCadastrosSapCommand(), CancellationToken.None),
        Cron.Daily());
}
await host.RunAsync();
