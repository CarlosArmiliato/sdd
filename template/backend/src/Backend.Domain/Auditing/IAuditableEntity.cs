namespace Backend.Domain.Auditing;

public interface IAuditableEntity
{
    DateTimeOffset CreationTime { get; }
    string CreatorUsername { get; }
    DateTimeOffset ModificationTime { get; }
    string ModifierUsername { get; }
}
