using Firmware.Api.Configuration;
using Firmware.Api.Endpoints;
using Firmware.Application.Abstractions;
using Firmware.Application.Health;
using Firmware.Infrastructure.Time;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddApiCors(builder.Configuration);
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddTransient<GetHealthStatus>();

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthEndpoints();
app.Run();

public partial class Program;
