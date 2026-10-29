namespace WebGate.Azure.FunctionsUtils.Internal;

/// <summary>
/// Reads Entra-related environment variables for local JWT validation (shared by API and SPA ID token parsers).
/// </summary>
internal static class LocalEntraJwtEnvironment
{
    internal const string TenantIdVariable = "FUNCTIONS_UTILS_AAD_TENANT_ID";
    internal const string ApiAudienceVariable = "FUNCTIONS_UTILS_AAD_AUDIENCE";
    internal const string SpaClientIdVariable = "FUNCTIONS_UTILS_AAD_SPA_CLIENT_ID";

    internal static string? GetTenantId()
    {
        return Environment.GetEnvironmentVariable(TenantIdVariable, EnvironmentVariableTarget.Process);
    }

    internal static string? GetApiAudience()
    {
        return Environment.GetEnvironmentVariable(ApiAudienceVariable, EnvironmentVariableTarget.Process);
    }

    internal static string? GetSpaClientId()
    {
        return Environment.GetEnvironmentVariable(SpaClientIdVariable, EnvironmentVariableTarget.Process);
    }
}
