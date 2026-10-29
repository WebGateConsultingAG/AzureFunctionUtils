using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace WebGate.Azure.FunctionsUtils.Internal;

/// <summary>
/// Parses Azure App Service Authentication / Easy Auth client principal from <c>x-ms-client-principal</c>.
/// </summary>
/// <remarks>
/// The header value is Base64-encoded JSON (standard Base64 or Base64url). Invalid or missing data returns <c>null</c> (fail closed).
/// Claim types are taken from the JSON <c>typ</c> field and must align with <see cref="ClaimsPrincipalResolver"/> (e.g. <c>oid</c>, <c>roles</c>).
/// </remarks>
internal static class EasyAuthClaimsPrincipalParser
{
    private const string CLIENT_PRINCIPAL_HEADER = "x-ms-client-principal";

    public static ClaimsPrincipal? TryParse(HttpRequest request)
    {
        if (!request.Headers.TryGetValue(CLIENT_PRINCIPAL_HEADER, out var headerValues))
        {
            return null;
        }

        try
        {
            var data = headerValues.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }

            var json = DecodeClientPrincipalPayload(data);
            var clientPrincipal = JsonSerializer.Deserialize<ClientPrincipal>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (clientPrincipal?.Claims == null)
            {
                return null;
            }

            var identity = new ClaimsIdentity(clientPrincipal.IdentityProvider);
            foreach (var claim in clientPrincipal.Claims)
            {
                if (claim.Type != null && claim.Value != null)
                {
                    identity.AddClaim(new Claim(claim.Type, claim.Value));
                }
            }

            return new ClaimsPrincipal(identity);
        }
        catch (FormatException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    internal static string DecodeClientPrincipalPayload(string encoded)
    {
        try
        {
            return Base64Url.DecodeToUtf8String(encoded);
        }
        catch (FormatException)
        {
            var decoded = Convert.FromBase64String(encoded);
            return Encoding.UTF8.GetString(decoded);
        }
    }
}
