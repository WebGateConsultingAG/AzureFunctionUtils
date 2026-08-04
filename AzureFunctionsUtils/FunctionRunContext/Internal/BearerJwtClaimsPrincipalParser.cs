using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace WebGate.Azure.FunctionsUtils.Internal;

internal static class BearerJwtClaimsPrincipalParser
{
    private const string AUTHORIZATION_HEADER = "Authorization";
    private const string BEARER_SCHEME = "Bearer";
    private const string TENANT_ID_VARIABLE = "FUNCTIONS_UTILS_AAD_TENANT_ID";
    private const string AUDIENCE_VARIABLE = "FUNCTIONS_UTILS_AAD_AUDIENCE";

    private static readonly ConcurrentDictionary<string, ConfigurationManager<OpenIdConnectConfiguration>> ConfigurationManagers = new();

    // Keep JWT claim types as short names (oid, roles, ...) so resolution matches Easy Auth.
    private static readonly JwtSecurityTokenHandler TokenHandler = new() { MapInboundClaims = false };

    public static ClaimsPrincipal? TryParse(HttpRequest request)
    {
        if (!AzureFunctionsEnvironment.IsLocalDevelopment())
        {
            return null;
        }

        if (!TryGetBearerToken(request, out var token))
        {
            return null;
        }

        var tenantId = Environment.GetEnvironmentVariable(TENANT_ID_VARIABLE, EnvironmentVariableTarget.Process);
        var audience = Environment.GetEnvironmentVariable(AUDIENCE_VARIABLE, EnvironmentVariableTarget.Process);
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(audience))
        {
            return null;
        }

        try
        {
            var openIdConfig = GetOpenIdConnectConfiguration(tenantId);
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers =
                [
                    $"https://login.microsoftonline.com/{tenantId}/v2.0",
                    $"https://sts.windows.net/{tenantId}/"
                ],
                ValidateAudience = true,
                ValidAudiences = BuildValidAudiences(audience),
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = openIdConfig.SigningKeys,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2)
            };

            return TokenHandler.ValidateToken(token, validationParameters, out _);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static bool TryGetBearerToken(HttpRequest request, out string token)
    {
        token = string.Empty;

        if (!request.Headers.TryGetValue(AUTHORIZATION_HEADER, out var headerValues))
        {
            return false;
        }

        var authorization = headerValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authorization))
        {
            return false;
        }

        var separatorIndex = authorization.IndexOf(' ');
        if (separatorIndex <= 0 ||
            !authorization[..separatorIndex].Equals(BEARER_SCHEME, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        token = authorization[(separatorIndex + 1)..].Trim();
        return token.Length > 0;
    }

    private static string[] BuildValidAudiences(string audience)
    {
        var audiences = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { audience };

        if (audience.StartsWith("api://", StringComparison.OrdinalIgnoreCase))
        {
            audiences.Add(audience["api://".Length..]);
        }
        else
        {
            audiences.Add($"api://{audience}");
        }

        return [.. audiences];
    }

    private static OpenIdConnectConfiguration GetOpenIdConnectConfiguration(string tenantId)
    {
        var metadataAddress =
            $"https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration";

        var manager = ConfigurationManagers.GetOrAdd(
            metadataAddress,
            address => new ConfigurationManager<OpenIdConnectConfiguration>(
                address,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever()));

        return manager.GetConfigurationAsync(CancellationToken.None).GetAwaiter().GetResult();
    }
}
