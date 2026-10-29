using System.Text.Json.Serialization;

namespace WebGate.Azure.FunctionsUtils.Internal.EasyAuth;

/// <summary>Deserialized Easy Auth client principal JSON (Azure Functions header).</summary>
internal sealed class ClientPrincipal
{
    /// <summary>Identity provider from <c>auth_typ</c> (used as <see cref="ClaimsIdentity"/> authentication type).</summary>
    [JsonPropertyName("auth_typ")]
    public string? IdentityProvider { get; set; }

    [JsonPropertyName("name_typ")]
    public string? NameClaimType { get; set; }

    [JsonPropertyName("role_typ")]
    public string? RoleClaimType { get; set; }

    /// <summary>Claim list deserialized into the resulting principal.</summary>
    [JsonPropertyName("claims")]
    public IEnumerable<ClientPrincipalClaim>? Claims { get; set; }
}
