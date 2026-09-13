using Backend.Domain.Authorization;

namespace Backend.App.Abstractions.Authorization;

public sealed class AuthorizationGrant
{
    public AuthorizationGrant(
        IReadOnlySet<string> permissions,
        IReadOnlySet<AuthorizationScope> scopes,
        IdentidadeAplicacao? applicationIdentity)
    {
        Permissions = permissions;
        Scopes = scopes;
        ApplicationIdentity = applicationIdentity;
    }

    public IReadOnlySet<string> Permissions { get; }
    public IReadOnlySet<AuthorizationScope> Scopes { get; }
    public IdentidadeAplicacao? ApplicationIdentity { get; }

    public bool HasPermission(string permission) => Permissions.Contains(permission);

    public bool HasScope(string scopeType, string scopeValue) => Scopes.Contains(new AuthorizationScope(scopeType, scopeValue));
}
