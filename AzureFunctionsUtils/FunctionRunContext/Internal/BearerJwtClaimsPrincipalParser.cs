using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace WebGate.Azure.FunctionsUtils.Internal;

/// <summary>
/// Local development: validates API access tokens using <see cref="LocalEntraJwtEnvironment.ApiAudienceVariable"/>.
/// </summary>
/// <remarks>Returns null when not in local dev mode or when tenant/audience env vars are missing (fail closed).</remarks>
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
