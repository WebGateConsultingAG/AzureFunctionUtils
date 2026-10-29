using System.Text.Json.Serialization;

namespace WebGate.Azure.FunctionsUtils.Internal.EasyAuth;

/// <summary>Single claim entry in an App Service Easy Auth <c>x-ms-client-principal</c> payload.</summary>
internal sealed class ClientPrincipalClaim
{
    /// <summary>Claim type in Easy Auth JSON (e.g. <c>oid</c>, <c>roles</c>).</summary>
    [JsonPropertyName("typ")]
    public string? Type { get; set; }

    /// <summary>Claim value in Easy Auth JSON.</summary>
    [JsonPropertyName("val")]
    public string? Value { get; set; }
}
