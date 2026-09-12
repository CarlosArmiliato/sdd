using Backend.App.Abstractions.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Backend.Infra.Postgres.Persistence.Auditing;

public sealed class AuditableSaveChangesInterceptor(
    TimeProvider timeProvider,
    IUserContext userContext) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Apply(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Apply(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Apply(DbContext? dbContext)
    {
        if (dbContext is null)
        {
            return;
        }
        dbContext.ChangeTracker.DetectChanges();
        DateTimeOffset timestamp = timeProvider.GetUtcNow();
        new AuditChangeApplier(dbContext, timestamp, userContext.Username).Apply();
    }
}
