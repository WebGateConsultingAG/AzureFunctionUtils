using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace WebGate.Azure.FunctionsUtils.Internal;

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

            var decoded = Convert.FromBase64String(data);
            var json = Encoding.UTF8.GetString(decoded);
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
}
