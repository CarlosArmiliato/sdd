namespace Backend.App.Identity;

public readonly record struct EntraObjectId
{
    public EntraObjectId(Guid value)
    {
        Value = RequireValue(value);
    }

    public Guid Value { get; }

    private static Guid RequireValue(Guid value) =>
        value != Guid.Empty
            ? value
            : throw new ArgumentException("O object ID do Microsoft Entra ID é obrigatório.", nameof(value));
}
