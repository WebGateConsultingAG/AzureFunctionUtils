using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using WebGate.Azure.FunctionsUtils.Internal.Environment;

namespace WebGate.Azure.FunctionsUtils.Internal.EntraJwt;

/// <summary>
/// Local development: validates SPA ID tokens (audience = SPA client id) when API access tokens are not sent.
/// </summary>
/// <remarks>
/// Runs only after <see cref="BearerJwtClaimsPrincipalParser"/> returns null.
/// Requires <see cref="LocalEntraJwtEnvironment.SpaClientIdVariable"/> and tenant id.
/// </remarks>
internal static class SpaIdTokenClaimsPrincipalParser
{
    /// <summary>
    /// Attempts SPA ID-token validation when local development and tenant/SPA client id env vars are set.
    /// </summary>
    /// <param name="request">Request carrying <c>Authorization: Bearer</c> (ID token, not access token).</param>
    /// <returns>Validated principal or null (including when not in local dev).</returns>
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
