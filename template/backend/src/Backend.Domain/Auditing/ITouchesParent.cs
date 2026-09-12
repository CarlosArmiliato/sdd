namespace Backend.Domain.Auditing;

public interface ITouchesParent<TParent, TKey>
{
    public TKey ParentId { get; }
}
