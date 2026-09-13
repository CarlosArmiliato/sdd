using Backend.App.Abstractions.Identity;
using Backend.App.Identity;
using Microsoft.AspNetCore.Http;

namespace Backend.Api.Identity;

public sealed class HttpUserContext(
    IHttpContextAccessor httpContextAccessor,
    ActorContextFactory actorContextFactory) : IUserContext
{
    private ActorContext Context => actorContextFactory.Create(httpContextAccessor.HttpContext?.User
        ?? throw new UnauthorizedAccessException("Não há contexto HTTP ativo."));

    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated is true;
    public bool IsApplication => IsAuthenticated && Context.ActorType is not ActorType.User;
    public string ActorId => Context.ActorId;
    public Guid? TenantId => Context.TenantId?.Value;
    public Guid? ObjectId => Context.ObjectId?.Value;
    public Guid? ClientId => Context.ClientId?.Value;
    public string Username => ActorId;
    public IReadOnlyCollection<string> Roles => Context.Roles;
    public IReadOnlyCollection<string> Scopes => Context.Scopes;
}
