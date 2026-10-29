using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using WebGate.Azure.FunctionsUtils.Internal.EasyAuth;
using WebGate.Azure.FunctionsUtils.Internal.EntraJwt;
using WebGate.Azure.FunctionsUtils.Internal.Environment;

namespace WebGate.Azure.FunctionsUtils.Internal.Resolution;

/// <summary>
/// Resolves <see cref="ClaimsPrincipal"/> and mapped identity fields from HTTP request headers.
/// </summary>
/// <remarks>
/// Cloud: Easy Auth only. Local dev (<see cref="AzureFunctionsEnvironment.IsLocalDevelopment"/>):
/// Easy Auth first, then API Bearer JWT, then SPA ID token JWT. No merging of principals.
/// </remarks>
internal static class ClaimsPrincipalResolver
{
    private const string OBJECT_ID_CLAIM = "oid";
    private const string OBJECT_ID_CLAIM_URI = "http://schemas.microsoft.com/identity/claims/objectidentifier";
    private const string SUBJECT_CLAIM = "sub";
    private const string NAME_IDENTIFIER_CLAIM_URI = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
    private const string UPN_CLAIM = "upn";
    private const string UPN_CLAIM_URI = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn";
    private const string PREFERRED_USERNAME_CLAIM = "preferred_username";
    private const string ROLES_CLAIM = "roles";
    private const string UNIQUE_NAME_CLAIM = "unique_name";
    private const string EMAIL_CLAIM = "email";

    /// <summary>Maps request headers to user id, UPN, and roles; returns empty identity when nothing valid is found.</summary>
    /// <param name="request">Incoming HTTP request (Easy Auth and, in local dev, Bearer JWT headers).</param>
    public static ResolvedIdentity Resolve(HttpRequest request)
    {
        var principal = EasyAuthClaimsPrincipalParser.TryParse(request);
        if (principal == null && AzureFunctionsEnvironment.IsLocalDevelopment())
        {
            principal = BearerJwtClaimsPrincipalParser.TryParse(request)
                ?? SpaIdTokenClaimsPrincipalParser.TryParse(request);
        }

        if (principal == null)
        {
            return new ResolvedIdentity();
        }

        return new ResolvedIdentity
        {
            Principal = principal,
            UserId = FindFirstClaimValue(
                principal,
                OBJECT_ID_CLAIM,
                OBJECT_ID_CLAIM_URI,
                SUBJECT_CLAIM,
                NAME_IDENTIFIER_CLAIM_URI),
            Upn = FindFirstClaimValue(
                principal,
                UPN_CLAIM,
                UPN_CLAIM_URI,
                PREFERRED_USERNAME_CLAIM,
                UNIQUE_NAME_CLAIM,
                EMAIL_CLAIM),
            Roles = FindAllClaimValues(principal, ROLES_CLAIM, ClaimTypes.Role)
        };
    }

    private static string[] FindAllClaimValues(ClaimsPrincipal principal, params string[] claimTypes)
    {
        return claimTypes
            .SelectMany(claimType => principal.FindAll(claimType))
            .Select(claim => claim.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string? FindFirstClaimValue(ClaimsPrincipal principal, params string[] claimTypes)
    {
        return claimTypes
            .Select(claimType => principal.FindFirst(claimType)?.Value)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }
}
