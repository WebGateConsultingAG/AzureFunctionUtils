using AzureFunctionsUtils.Tests.Infrastructure;
using Microsoft.AspNetCore.Http;
using WebGate.Azure.FunctionsUtils.Internal.Resolution;
using Xunit;

namespace AzureFunctionsUtils.Tests.Resolution;

[Collection(nameof(EnvironmentVariablesCollection))]
public class ClaimsPrincipalResolverTests
{
    [Fact]
    public void Resolve_withoutAuthHeaders_returnsUnauthenticatedIdentity()
    {
        using var local = new EnvironmentVariableRestore(
            AzureFunctionsEnvironmentTests.LocalDevelopmentVariable,
            null);

        var context = new DefaultHttpContext();
        var resolved = ClaimsPrincipalResolver.Resolve(context.Request);

        Assert.Null(resolved.Principal);
        Assert.False(resolved.IsAuthenticated);
        Assert.True(string.IsNullOrWhiteSpace(resolved.UserId));
    }
}
