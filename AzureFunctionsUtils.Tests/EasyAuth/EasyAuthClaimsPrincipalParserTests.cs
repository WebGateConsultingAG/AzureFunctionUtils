using AzureFunctionsUtils.Tests.Infrastructure;
using System.Text;
using Microsoft.AspNetCore.Http;
using WebGate.Azure.FunctionsUtils.Internal.EasyAuth;
using WebGate.Azure.FunctionsUtils.Internal.Resolution;
using Xunit;

namespace AzureFunctionsUtils.Tests.EasyAuth;

[Collection(nameof(EnvironmentVariablesCollection))]
public class EasyAuthClaimsPrincipalParserTests
{
    [Fact]
    public void DecodeClientPrincipalPayload_acceptsStandardBase64()
    {
        var json = """{"auth_typ":"aad","claims":[{"typ":"oid","val":"oid-1"}]}""";
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        var decoded = EasyAuthClaimsPrincipalParser.DecodeClientPrincipalPayload(encoded);

        Assert.Contains("oid-1", decoded, StringComparison.Ordinal);
    }

    [Fact]
    public void TryParse_resolvesUserIdFromOidClaim()
    {
        var json = """{"auth_typ":"aad","claims":[{"typ":"oid","val":"oid-easy-auth"}]}""";
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        var context = new DefaultHttpContext();
        context.Request.Headers["x-ms-client-principal"] = encoded;

        var principal = EasyAuthClaimsPrincipalParser.TryParse(context.Request);
        var resolved = ClaimsPrincipalResolver.Resolve(context.Request);

        Assert.NotNull(principal);
        Assert.Equal("oid-easy-auth", resolved.UserId);
    }
}
