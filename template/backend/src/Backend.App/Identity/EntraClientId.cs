namespace Backend.App.Identity;

public readonly record struct EntraClientId
{
    public EntraClientId(Guid value)
    {
        Value = RequireValue(value);
    }

    public Guid Value { get; }

    private static Guid RequireValue(Guid value) =>
        value != Guid.Empty
            ? value
            : throw new ArgumentException("O client ID do Microsoft Entra ID é obrigatório.", nameof(value));
}
