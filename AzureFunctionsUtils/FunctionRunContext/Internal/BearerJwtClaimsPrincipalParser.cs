using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace WebGate.Azure.FunctionsUtils.Internal;

internal static class BearerJwtClaimsPrincipalParser
{
    public static ClaimsPrincipal? TryParse(HttpRequest request)
    {
        if (!AzureFunctionsEnvironment.IsLocalDevelopment())
        {
            return null;
        }

        var tenantId = LocalEntraJwtEnvironment.GetTenantId();
        var audience = LocalEntraJwtEnvironment.GetApiAudience();
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(audience))
        {
            return null;
        }

        return EntraJwtClaimsPrincipalParser.TryParse(
            request,
            tenantId,
            EntraJwtClaimsPrincipalParser.BuildValidAudiences(audience));
    }
}
