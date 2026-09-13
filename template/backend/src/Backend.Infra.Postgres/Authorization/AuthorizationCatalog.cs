using Backend.App.Abstractions.Authorization;
using Backend.Domain.Authorization;
using Backend.Infra.Postgres.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infra.Postgres.Authorization;

public sealed class AuthorizationCatalog(BackendDbContext dbContext) : IAuthorizationCatalog
{
    public async Task<AuthorizationGrant> ResolveAsync(AuthorizationCatalogRequest request, CancellationToken cancellationToken)
    {
        HashSet<string> roles = request.Roles.Where(x => !string.IsNullOrWhiteSpace(x)).ToHashSet(StringComparer.Ordinal);
        IdentidadeAplicacao? identity = await FindApplicationAsync(request, cancellationToken);
        if (request.IsApplication && identity is null)
        {
            return new AuthorizationGrant(new HashSet<string>(), new HashSet<AuthorizationScope>(), null);
        }
        HashSet<string> permissions = await FindPermissionsAsync(roles, request.IsApplication, cancellationToken);
        HashSet<AuthorizationScope> scopes = await FindScopesAsync(roles, cancellationToken);
        return new AuthorizationGrant(permissions, scopes, identity);
    }

    private Task<IdentidadeAplicacao?> FindApplicationAsync(AuthorizationCatalogRequest request, CancellationToken cancellationToken)
    {
        if (!request.IsApplication || request.ClientId is not Guid clientId)
        {
            return Task.FromResult<IdentidadeAplicacao?>(null);
        }
        return dbContext.IdentidadesAplicacao.AsNoTracking().SingleOrDefaultAsync(
            x => x.TenantId == request.TenantId && x.ObjectId == request.ObjectId && x.ClientId == clientId && x.Ativa,
            cancellationToken);
    }

    private async Task<HashSet<string>> FindPermissionsAsync(HashSet<string> roles, bool isApplication, CancellationToken cancellationToken)
    {
        RoleSubjectType subjectType = isApplication ? RoleSubjectType.Application : RoleSubjectType.User;
        return await (from role in dbContext.MapeamentosRolePerfil.AsNoTracking()
                      join profile in dbContext.PerfisAcesso.AsNoTracking() on role.PerfilId equals profile.Id
                      join permission in dbContext.PerfisPermissao.AsNoTracking() on profile.Id equals permission.PerfilId
                      join definition in dbContext.Permissoes.AsNoTracking() on permission.PermissaoId equals definition.Id
                      where roles.Contains(role.RoleValue) && role.Ativo && profile.Ativo
                          && (role.SubjectType == subjectType || role.SubjectType == RoleSubjectType.Both)
                      select definition.Codigo).ToHashSetAsync(StringComparer.Ordinal, cancellationToken);
    }

    private Task<HashSet<AuthorizationScope>> FindScopesAsync(HashSet<string> roles, CancellationToken cancellationToken) =>
        dbContext.MapeamentosRoleEscopo.AsNoTracking()
            .Where(x => roles.Contains(x.RoleValue) && x.Ativo)
            .Select(x => new AuthorizationScope(x.ScopeType, x.ScopeValue))
            .ToHashSetAsync(cancellationToken);
}
