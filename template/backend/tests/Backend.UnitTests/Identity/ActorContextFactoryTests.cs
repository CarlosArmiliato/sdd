using System.Security.Claims;
using Backend.Api.Identity;
using Backend.App.Identity;
using Microsoft.Extensions.Options;

namespace Backend.UnitTests.Identity;

public sealed class ActorContextFactoryTests
{
    private static readonly Guid TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ObjectId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid ClientId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Fact]
    public void CreateBuildsCanonicalActorIdForDelegatedUser()
    {
        ActorContext context = CreateFactory().Create(CreatePrincipal(("idtyp", "user"), ("tid", TenantId.ToString()), ("oid", ObjectId.ToString()), ("scp", "access_as_user")));

        Assert.Equal($"v1:entra:user:{TenantId:D}:{ObjectId:D}", context.ActorId);
        Assert.Equal(ActorType.User, context.ActorType);
        Assert.Equal(TenantId, context.TenantId?.Value);
        Assert.Empty(context.Roles);
    }

    [Fact]
    public void CreateBuildsCanonicalActorIdForApplication()
    {
        ActorContext context = CreateFactory().Create(CreatePrincipal(("idtyp", "app"), ("tid", TenantId.ToString()), ("oid", ObjectId.ToString()), ("azp", ClientId.ToString()), ("roles", "Cadastros.Integration.Write")));

        Assert.Equal($"v1:entra:app:{TenantId:D}:{ObjectId:D}", context.ActorId);
        Assert.Equal(ActorType.InternalApplication, context.ActorType);
        Assert.Equal(ClientId, context.ClientId?.Value);
        Assert.Equal(["Cadastros.Integration.Write"], context.Roles);
    }

    [Theory]
    [InlineData("tid")]
    [InlineData("oid")]
    [InlineData("idtyp")]
    public void CreateRejectsIncompleteOrAmbiguousClaims(string omittedClaim)
    {
        (string Type, string Value)[] claims = [("idtyp", "app"), ("tid", TenantId.ToString()), ("oid", ObjectId.ToString()), ("azp", ClientId.ToString()), ("roles", "Cadastros.Integration.Write")];
        ClaimsPrincipal incompletePrincipal = CreatePrincipal(claims.Where(claim => claim.Type != omittedClaim).ToArray());
        ClaimsPrincipal ambiguousPrincipal = CreatePrincipal(("idtyp", "app"), ("tid", TenantId.ToString()), ("oid", ObjectId.ToString()), ("azp", ClientId.ToString()), ("appid", Guid.NewGuid().ToString()), ("roles", "Cadastros.Integration.Write"));

        Assert.Throws<UnauthorizedAccessException>(() => CreateFactory().Create(incompletePrincipal));
        Assert.Throws<UnauthorizedAccessException>(() => CreateFactory().Create(ambiguousPrincipal));
    }

    private static ActorContextFactory CreateFactory() =>
        new(Options.Create(new ActorIdentityOptions()));

    private static ClaimsPrincipal CreatePrincipal(params (string Type, string Value)[] claims) =>
        new(new ClaimsIdentity(claims.Select(claim => new Claim(claim.Type, claim.Value)), "test"));
}
