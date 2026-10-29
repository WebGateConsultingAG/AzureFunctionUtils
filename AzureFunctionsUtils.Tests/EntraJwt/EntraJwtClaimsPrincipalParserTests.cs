using WebGate.Azure.FunctionsUtils.Internal.EntraJwt;
using Xunit;

namespace AzureFunctionsUtils.Tests.EntraJwt;

public class EntraJwtClaimsPrincipalParserTests
{
    [Fact]
    public void BuildValidAudiences_withApiPrefix_includesBareApplicationId()
    {
        var audiences = EntraJwtClaimsPrincipalParser.BuildValidAudiences("api://11111111-2222-3333-4444-555555555555");

        Assert.Equal(2, audiences.Length);
        Assert.Contains("api://11111111-2222-3333-4444-555555555555", audiences, StringComparer.Ordinal);
        Assert.Contains("11111111-2222-3333-4444-555555555555", audiences, StringComparer.Ordinal);
    }

    [Fact]
    public void BuildValidAudiences_withBareId_includesApiPrefixForm()
    {
        var audiences = EntraJwtClaimsPrincipalParser.BuildValidAudiences("11111111-2222-3333-4444-555555555555");

        Assert.Equal(2, audiences.Length);
        Assert.Contains("api://11111111-2222-3333-4444-555555555555", audiences, StringComparer.Ordinal);
        Assert.Contains("11111111-2222-3333-4444-555555555555", audiences, StringComparer.Ordinal);
    }

    [Fact]
    public void BuildValidAudiences_isCaseInsensitiveForDuplicateForms()
    {
        var audiences = EntraJwtClaimsPrincipalParser.BuildValidAudiences("API://MyAppId");

        Assert.Equal(2, audiences.Length);
        Assert.Contains("API://MyAppId", audiences, StringComparer.Ordinal);
        Assert.Contains("MyAppId", audiences, StringComparer.OrdinalIgnoreCase);
    }
}
