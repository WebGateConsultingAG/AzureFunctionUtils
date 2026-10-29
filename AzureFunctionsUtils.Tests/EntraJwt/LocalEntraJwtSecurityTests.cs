using AzureFunctionsUtils.Tests.Infrastructure;
using Microsoft.AspNetCore.Http;
using WebGate.Azure.FunctionsUtils.Internal.EntraJwt;
using WebGate.Azure.FunctionsUtils.Internal.Environment;
using WebGate.Azure.FunctionsUtils.Internal.Resolution;
using Xunit;

namespace AzureFunctionsUtils.Tests.EntraJwt;

/// <summary>
/// Fail-closed and trust-boundary tests for local JWT parsers (no live OIDC calls).
/// </summary>
[Collection(nameof(EnvironmentVariablesCollection))]
public class LocalEntraJwtSecurityTests
{
    [Fact]
    public void Resolve_withBearerHeader_whenLocalDevelopmentDisabled_staysUnauthenticated()
    {
        using var _ = new EnvironmentVariableRestore(LocalEntraJwtEnvironment.TenantIdVariable, "00000000-0000-0000-0000-000000000001");
        using var __ = new EnvironmentVariableRestore(LocalEntraJwtEnvironment.ApiAudienceVariable, "api://test");
        using var ___ = new EnvironmentVariableRestore(
            AzureFunctionsEnvironmentTests.LocalDevelopmentVariable,
            "false");

        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = "Bearer not-a-real-jwt";

        var resolved = ClaimsPrincipalResolver.Resolve(context.Request);

        Assert.Null(resolved.Principal);
        Assert.False(resolved.IsAuthenticated);
    }

    [Fact]
    public void BearerJwtClaimsPrincipalParser_returnsNull_whenLocalDevelopmentDisabled()
    {
        using var _ = new EnvironmentVariableRestore(
            AzureFunctionsEnvironmentTests.LocalDevelopmentVariable,
            null);

        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = "Bearer eyJhbGciOiJub25lIn0.eyJzdWIiOiJ4In0.";

        Assert.Null(BearerJwtClaimsPrincipalParser.TryParse(context.Request));
    }

    [Fact]
    public void SpaIdTokenClaimsPrincipalParser_returnsNull_whenLocalDevelopmentDisabled()
    {
        using var _ = new EnvironmentVariableRestore(
            AzureFunctionsEnvironmentTests.LocalDevelopmentVariable,
            null);
        using var __ = new EnvironmentVariableRestore(LocalEntraJwtEnvironment.TenantIdVariable, "00000000-0000-0000-0000-000000000001");
        using var ___ = new EnvironmentVariableRestore(LocalEntraJwtEnvironment.SpaClientIdVariable, "spa-client-id");

        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = "Bearer eyJhbGciOiJub25lIn0.eyJzdWIiOiJ4In0.";

        Assert.Null(SpaIdTokenClaimsPrincipalParser.TryParse(context.Request));
    }

    [Fact]
    public void BearerJwtClaimsPrincipalParser_returnsNull_whenTenantOrAudienceMissing()
    {
        using var local = new EnvironmentVariableRestore(
            AzureFunctionsEnvironmentTests.LocalDevelopmentVariable,
            "true");
        using var tenant = new EnvironmentVariableRestore(LocalEntraJwtEnvironment.TenantIdVariable, null);
        using var audience = new EnvironmentVariableRestore(LocalEntraJwtEnvironment.ApiAudienceVariable, null);

        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = "Bearer eyJhbGciOiJub25lIn0.eyJzdWIiOiJ4In0.";

        Assert.Null(BearerJwtClaimsPrincipalParser.TryParse(context.Request));
    }

    [Fact]
    public void SpaIdTokenClaimsPrincipalParser_returnsNull_whenSpaClientIdMissing()
    {
        using var local = new EnvironmentVariableRestore(
            AzureFunctionsEnvironmentTests.LocalDevelopmentVariable,
            "true");
        using var tenant = new EnvironmentVariableRestore(
            LocalEntraJwtEnvironment.TenantIdVariable,
            "00000000-0000-0000-0000-000000000001");
        using var spa = new EnvironmentVariableRestore(LocalEntraJwtEnvironment.SpaClientIdVariable, null);

        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = "Bearer eyJhbGciOiJub25lIn0.eyJzdWIiOiJ4In0.";

        Assert.Null(SpaIdTokenClaimsPrincipalParser.TryParse(context.Request));
    }

    [Fact]
    public void Resolve_prefersEasyAuth_overBearer_whenBothPresent()
    {
        using var local = new EnvironmentVariableRestore(
            AzureFunctionsEnvironmentTests.LocalDevelopmentVariable,
            "true");
        using var tenant = new EnvironmentVariableRestore(
            LocalEntraJwtEnvironment.TenantIdVariable,
            "00000000-0000-0000-0000-000000000001");
        using var audience = new EnvironmentVariableRestore(LocalEntraJwtEnvironment.ApiAudienceVariable, "api://test");

        var json = """{"auth_typ":"aad","claims":[{"typ":"oid","val":"oid-from-easy-auth"}]}""";
        var encoded = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));

        var context = new DefaultHttpContext();
        context.Request.Headers["x-ms-client-principal"] = encoded;
        context.Request.Headers.Authorization = "Bearer eyJhbGciOiJub25lIn0.eyJzdWIiOiJ4In0.";

        var resolved = ClaimsPrincipalResolver.Resolve(context.Request);

        Assert.Equal("oid-from-easy-auth", resolved.UserId);
    }
}
