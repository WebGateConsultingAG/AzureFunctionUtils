using System.Text;
using Microsoft.AspNetCore.Http;
using WebGate.Azure.FunctionsUtils;
using WebGate.Azure.FunctionsUtils.Internal;
using Xunit;

namespace AzureFunctionsUtils.Tests;

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

public class ClaimsPrincipalResolverTests
{
    [Fact]
    public void Resolve_withoutAuthHeaders_returnsUnauthenticatedIdentity()
    {
        Environment.SetEnvironmentVariable("FUNCTIONS_UTILS_LOCAL_DEVELOPMENT", null);

        var context = new DefaultHttpContext();
        var resolved = ClaimsPrincipalResolver.Resolve(context.Request);

        Assert.Null(resolved.Principal);
        Assert.False(resolved.IsAuthenticated);
        Assert.True(string.IsNullOrWhiteSpace(resolved.UserId));
    }
}

public class UserFunctionRunContextTests
{
    [Fact]
    public void ForRequest_returnsSameInstanceFromHttpContextItems()
    {
        var context = new DefaultHttpContext();

        var first = UserFunctionRunContext.ForRequest(context.Request);
        var second = UserFunctionRunContext.ForRequest(context.Request);

        Assert.Same(first, second);
    }
}
