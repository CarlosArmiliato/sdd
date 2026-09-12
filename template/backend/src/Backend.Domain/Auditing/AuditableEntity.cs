namespace Backend.Domain.Auditing;

public abstract class AuditableEntity : IAuditableEntity
{
    public DateTimeOffset CreationTime { get; private set; }
    public string CreatorUsername { get; private set; } = string.Empty;
    public DateTimeOffset ModificationTime { get; private set; }
    public string ModifierUsername { get; private set; } = string.Empty;
}
