namespace Backend.App.Abstractions.Authorization;

public interface IAuthorizationCatalog
{
    public Task<AuthorizationGrant> ResolveAsync(AuthorizationCatalogRequest request, CancellationToken cancellationToken);
}
