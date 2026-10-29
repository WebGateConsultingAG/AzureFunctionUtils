using System.Text.Json.Serialization;

namespace WebGate.Azure.FunctionsUtils.Internal;

/// <summary>Single claim entry in an App Service Easy Auth <c>x-ms-client-principal</c> payload.</summary>
internal sealed class ClientPrincipalClaim
{
    [JsonPropertyName("typ")]
    public string? Type { get; set; }

    [JsonPropertyName("val")]
    public string? Value { get; set; }
}

/// <summary>Deserialized Easy Auth client principal JSON (Azure Functions header).</summary>
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
