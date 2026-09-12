using Backend.Api.Configuration;
using Backend.Api.Endpoints;
using Backend.Application.Abstractions;
using Backend.Application.Health;
using Backend.Infrastructure.Time;

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
