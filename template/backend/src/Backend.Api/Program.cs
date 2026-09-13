using Backend.Api.Configuration;
using Backend.Api.Filters;
using Backend.Api.Identity;
using Backend.App;
using Backend.App.Abstractions;
using Backend.App.Abstractions.Identity;
using Backend.App.Health;
using Backend.Infra.Hangfire;
using Backend.Infra.Postgres;
using Backend.Infra.Redis;
using Backend.App.Time;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();

builder.Services.AddOpenApi();
builder.Services.AddControllers(options => options.Filters.Add<ResponseResultFilter>());
builder.Services.AddProblemDetails();
builder.Services.AddApiCors(builder.Configuration);
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddTransient<GetHealthStatus>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, HttpUserContext>();
builder.Services.AddSingleton<ActorContextFactory>();
builder.Services.AddOptions<ActorIdentityOptions>()
    .Bind(builder.Configuration.GetSection(ActorIdentityOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.UserTokenType), "ActorIdentity:UserTokenType é obrigatório.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.ApplicationTokenType), "ActorIdentity:ApplicationTokenType é obrigatório.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.RequiredDelegatedScope), "ActorIdentity:RequiredDelegatedScope é obrigatório.")
    .Validate(options => options.UserTokenType != options.ApplicationTokenType, "Os tipos de token devem ser distintos.")
    .ValidateOnStart();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddAuthorization(options => options.AddPolicy(
    AuthorizationPolicies.BackendUser,
    policy => policy.RequireAuthenticatedUser()));

if (builder.Configuration.GetValue("ExternalServices:Enabled", true))
{
    builder.Services.AddApp();
    string postgres = builder.Configuration.GetConnectionString("Postgres")
        ?? throw new InvalidOperationException("ConnectionStrings:Postgres não configurada.");
    string redis = builder.Configuration.GetConnectionString("Redis")
        ?? throw new InvalidOperationException("ConnectionStrings:Redis não configurada.");
    builder.Services.AddPostgres(postgres);
    builder.Services.AddRedis(redis);
    builder.Services.AddHangfireClient(postgres);
}

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithTitle("Backend API"));
}

app.MapControllers();
await app.RunAsync();

public partial class Program;
