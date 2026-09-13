using System.Security.Claims;
using Backend.App.Identity;
using Microsoft.Extensions.Options;

namespace Backend.Api.Identity;

public sealed class ActorContextFactory(IOptions<ActorIdentityOptions> options)
{
    private const string TenantIdClaim = "tid";
    private const string ObjectIdClaim = "oid";
    private const string TokenTypeClaim = "idtyp";
    private const string AuthorizedPartyClaim = "azp";
    private const string ApplicationIdClaim = "appid";
    private const string RoleClaim = "roles";
    private const string ScopeClaim = "scp";

    private readonly ActorIdentityOptions _options = options.Value;

    public ActorContext Create(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        EntraTenantId tenantId = new(ParseGuid(principal, TenantIdClaim));
        EntraObjectId objectId = new(ParseGuid(principal, ObjectIdClaim));
        string tokenType = GetRequiredClaim(principal, TokenTypeClaim);
        return tokenType switch
        {
            var value when value == _options.UserTokenType => CreateUser(principal, tenantId, objectId),
            var value when value == _options.ApplicationTokenType => CreateApplication(principal, tenantId, objectId),
            _ => throw new UnauthorizedAccessException("O tipo de token não é aceito.")
        };
    }

    private ActorContext CreateUser(ClaimsPrincipal principal, EntraTenantId tenantId, EntraObjectId objectId)
    {
        string[] scopes = GetValues(principal, ScopeClaim).SelectMany(value => value.Split(' ', StringSplitOptions.RemoveEmptyEntries)).ToArray();
        if (!scopes.Contains(_options.RequiredDelegatedScope, StringComparer.Ordinal))
        {
            throw new UnauthorizedAccessException("O token delegado não possui o escopo obrigatório.");
        }
        return ActorContext.CreateUser(tenantId, objectId, GetValues(principal, RoleClaim), scopes);
    }

    private static ActorContext CreateApplication(ClaimsPrincipal principal, EntraTenantId tenantId, EntraObjectId objectId)
    {
        Guid clientId = ParseApplicationId(principal);
        string[] roles = GetValues(principal, RoleClaim).ToArray();
        if (roles.Length == 0)
        {
            throw new UnauthorizedAccessException("O token de aplicação não possui app roles.");
        }
        return ActorContext.CreateApplication(tenantId, objectId, new EntraClientId(clientId), ActorType.InternalApplication, roles);
    }

    private static Guid ParseGuid(ClaimsPrincipal principal, string claimType) =>
        Guid.TryParse(GetRequiredClaim(principal, claimType), out Guid value) && value != Guid.Empty
            ? value
            : throw new UnauthorizedAccessException($"A claim {claimType} é inválida.");

    private static Guid ParseApplicationId(ClaimsPrincipal principal)
    {
        string? azp = GetOptionalClaim(principal, AuthorizedPartyClaim);
        string? appId = GetOptionalClaim(principal, ApplicationIdClaim);
        if (azp is not null && appId is not null && !string.Equals(azp, appId, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("As claims de aplicação são ambíguas.");
        }
        if (!Guid.TryParse(azp ?? appId, out Guid clientId) || clientId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("A identidade da aplicação é inválida.");
        }
        return clientId;
    }

    private static string GetRequiredClaim(ClaimsPrincipal principal, string claimType) =>
        GetOptionalClaim(principal, claimType)
            ?? throw new UnauthorizedAccessException($"A claim {claimType} é obrigatória.");

    private static string? GetOptionalClaim(ClaimsPrincipal principal, string claimType)
    {
        string[] values = GetValues(principal, claimType).Distinct(StringComparer.Ordinal).ToArray();
        return values.Length switch
        {
            0 => null,
            1 => values[0],
            _ => throw new UnauthorizedAccessException($"A claim {claimType} é ambígua.")
        };
    }

    private static IEnumerable<string> GetValues(ClaimsPrincipal principal, string claimType) =>
        principal.FindAll(claimType).Select(claim => claim.Value).Where(value => !string.IsNullOrWhiteSpace(value));
}
