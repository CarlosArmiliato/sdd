namespace Backend.Api.Identity;

public sealed class ActorIdentityOptions
{
    public const string SectionName = "ActorIdentity";

    public string UserTokenType { get; init; } = "user";
    public string ApplicationTokenType { get; init; } = "app";
    public string RequiredDelegatedScope { get; init; } = "access_as_user";
}
