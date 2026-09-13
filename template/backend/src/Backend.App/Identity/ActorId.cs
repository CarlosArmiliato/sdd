namespace Backend.App.Identity;

public sealed record ActorId
{
    private const string VersionPrefix = "v1:";
    private const string EntraPrefix = "v1:entra:";
    private const string SystemPrefix = "v1:system:";

    private ActorId(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ActorId CreateUser(EntraTenantId tenantId, EntraObjectId objectId) =>
        new($"{EntraPrefix}user:{tenantId.Value:D}:{objectId.Value:D}");

    public static ActorId CreateApplication(EntraTenantId tenantId, EntraObjectId objectId) =>
        new($"{EntraPrefix}app:{tenantId.Value:D}:{objectId.Value:D}");

    public static ActorId CreateSystem(string host, string process) =>
        new($"{SystemPrefix}{RequireSystemSegment(host, nameof(host))}:{RequireSystemSegment(process, nameof(process))}");

    public static ActorId Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (!value.StartsWith(VersionPrefix, StringComparison.Ordinal))
        {
            return new(value);
        }
        if (TryParseEntra(value) || TryParseSystem(value))
        {
            return new(value);
        }
        throw new ArgumentException("O ActorId versionado é inválido.", nameof(value));
    }

    public override string ToString() => Value;

    private static bool TryParseEntra(string value)
    {
        string[] segments = value.Split(':', StringSplitOptions.None);
        return segments.Length == 5
            && segments[0] == "v1"
            && segments[1] == "entra"
            && (segments[2] == "user" || segments[2] == "app")
            && Guid.TryParseExact(segments[3], "D", out Guid tenantId)
            && tenantId != Guid.Empty
            && Guid.TryParseExact(segments[4], "D", out Guid objectId)
            && objectId != Guid.Empty;
    }

    private static bool TryParseSystem(string value)
    {
        string[] segments = value.Split(':', StringSplitOptions.None);
        return segments.Length == 4
            && segments[0] == "v1"
            && segments[1] == "system"
            && IsValidSystemSegment(segments[2])
            && IsValidSystemSegment(segments[3]);
    }

    private static string RequireSystemSegment(string value, string parameterName) =>
        IsValidSystemSegment(value)
            ? value
            : throw new ArgumentException("O segmento de identidade do processo é inválido.", parameterName);

    private static bool IsValidSystemSegment(string value) =>
        !string.IsNullOrWhiteSpace(value) && !value.Contains(':') && value.Length <= 120;
}
