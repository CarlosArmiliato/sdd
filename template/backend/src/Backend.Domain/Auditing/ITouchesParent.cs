namespace Backend.Domain.Auditing;

public interface ITouchesParent<TParent, TKey>
{
    TKey ParentId { get; }
}
