namespace Backend.Domain.Auditing;

public interface IAuditableEntity
{
    public DateTimeOffset CreationTime { get; }
    public string CreatorUsername { get; }
    public DateTimeOffset ModificationTime { get; }
    public string ModifierUsername { get; }
}
