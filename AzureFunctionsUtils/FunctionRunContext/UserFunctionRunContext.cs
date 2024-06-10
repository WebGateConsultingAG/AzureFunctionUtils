using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace WebGate.Azure.FunctionsUtils;
public class UserFunctionRunContext : FunctionRunContext
{
    private ClaimsPrincipal? _principal;
    public UserFunctionRunContext(HttpRequest request) : base(FunctionRunContextType.USER)
    {
        _request = request;
        string? env = GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");
        bool isDevSet = string.IsNullOrEmpty(GetEnvironmentVariable("IS_NOT_DEV"));

        if (env == "Development" && isDevSet)
        {
            _isDev = true;
            string? envUserId = GetEnvironmentVariable("DEV_USER_ID");
            string? rolesByEnvironment = GetEnvironmentVariable("DEV_USER_ROLES");
            if (string.IsNullOrEmpty(envUserId))
            {
                _userId = "LocalDev";
            }
            else
            {
                _userId = envUserId;
            }
            _authenticated = true;
            string[] allRoles = rolesByEnvironment != null ? rolesByEnvironment.Split(",") : ["admin"];
            _roles = new List<string>(allRoles);
        }
        else
        {
            _principal = CheckInitializeClaimsPrincipal();
            if (_principal != null)
            {
                string? userId = _principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    _userId = userId;
                    _authenticated = true;
                }
                _upn = _principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn")?.Value;
                _roles = _principal.Claims.Where(e => e.Type == "roles").Select(e => e.Value);
            }
            else
            {
                _authenticated = false;
            }
        }
    }
    private ClaimsPrincipal? CheckInitializeClaimsPrincipal()
    {
        if (_request != null && _request.Headers.TryGetValue("x-ms-client-principal", out var header))
        {
            var data = header.First();
            var decoded = Convert.FromBase64String(data!);
            var json = Encoding.UTF8.GetString(decoded);
            var clientPrincipal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (clientPrincipal != null)
            {
                var identity = new ClaimsIdentity(clientPrincipal.IdentityProvider);
                foreach (var claimId in clientPrincipal.Claims!)
                {
                    identity.AddClaim(new Claim(claimId.Type!, claimId.Value!));

                }
                return new ClaimsPrincipal(identity);
            }
            return null;
        }
        else
        {
            return null;
        }
    }
}

public class ClientPrincipalClaim
{
    [JsonPropertyName("typ")]
    public string? Type { get; set; }
    [JsonPropertyName("val")]
    public string? Value { get; set; }
}

public class ClientPrincipal
{
    [JsonPropertyName("auth_typ")]
    public string? IdentityProvider { get; set; }
    [JsonPropertyName("name_typ")]
    public string? NameClaimType { get; set; }
    [JsonPropertyName("role_typ")]
    public string? RoleClaimType { get; set; }
    [JsonPropertyName("claims")]
    public IEnumerable<ClientPrincipalClaim>? Claims { get; set; }
}
