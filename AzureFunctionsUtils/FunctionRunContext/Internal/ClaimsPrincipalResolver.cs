using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace WebGate.Azure.FunctionsUtils.Internal;

internal sealed class ResolvedIdentity
{
    public ClaimsPrincipal? Principal { get; init; }
    public string? UserId { get; init; }
    public string? Upn { get; init; }
    public IEnumerable<string> Roles { get; init; } = [];
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(UserId);
}

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

    public static ResolvedIdentity Resolve(HttpRequest request)
    {
        var principal = EasyAuthClaimsPrincipalParser.TryParse(request);
        if (principal == null && AzureFunctionsEnvironment.IsLocalDevelopment())
        {
            principal = BearerJwtClaimsPrincipalParser.TryParse(request);
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
                PREFERRED_USERNAME_CLAIM),
            Roles = principal.FindAll(ROLES_CLAIM).Select(claim => claim.Value).ToArray()
        };
    }

    private static string? FindFirstClaimValue(ClaimsPrincipal principal, params string[] claimTypes)
    {
        return claimTypes
            .Select(claimType => principal.FindFirst(claimType)?.Value)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }
}
