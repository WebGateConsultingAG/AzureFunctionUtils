using System.Text.Json.Serialization;

namespace WebGate.Azure.FunctionsUtils.Internal;

internal sealed class ClientPrincipalClaim
{
    [JsonPropertyName("typ")]
    public string? Type { get; set; }

    [JsonPropertyName("val")]
    public string? Value { get; set; }
}

internal sealed class ClientPrincipal
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
