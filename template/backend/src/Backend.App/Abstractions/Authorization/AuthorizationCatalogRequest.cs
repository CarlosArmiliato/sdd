namespace Backend.App.Abstractions.Authorization;

public sealed record AuthorizationCatalogRequest(
    Guid TenantId,
    Guid ObjectId,
    Guid? ClientId,
    bool IsApplication,
    IReadOnlyCollection<string> Roles);
