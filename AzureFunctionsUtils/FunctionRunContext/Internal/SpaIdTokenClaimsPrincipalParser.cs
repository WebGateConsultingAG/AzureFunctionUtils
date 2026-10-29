using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace WebGate.Azure.FunctionsUtils.Internal;

/// <summary>
/// Local development: validates SPA ID tokens (audience = SPA client id) when API access tokens are not sent.
/// </summary>
internal static class SpaIdTokenClaimsPrincipalParser
{
    public static ClaimsPrincipal? TryParse(HttpRequest request)
    {
        if (!AzureFunctionsEnvironment.IsLocalDevelopment())
        {
            return null;
        }

        var tenantId = LocalEntraJwtEnvironment.GetTenantId();
        var spaClientId = LocalEntraJwtEnvironment.GetSpaClientId();
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(spaClientId))
        {
            return null;
        }

        return EntraJwtClaimsPrincipalParser.TryParse(
            request,
            tenantId,
            EntraJwtClaimsPrincipalParser.BuildValidAudiences(spaClientId));
    }
}
