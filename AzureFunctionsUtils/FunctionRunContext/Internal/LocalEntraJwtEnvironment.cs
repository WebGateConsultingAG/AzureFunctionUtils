namespace WebGate.Azure.FunctionsUtils.Internal;

/// <summary>
/// Reads Entra-related environment variables for local JWT validation (shared by API and SPA ID token parsers).
/// </summary>
/// <remarks>
/// All variables are process-scoped (Azure Functions <c>local.settings.json</c> Values).
/// Missing tenant or audience/client id causes parsers to return null (fail closed).
/// </remarks>
internal static class LocalEntraJwtEnvironment
{
    /// <summary>Entra directory (tenant) id for OpenID metadata and issuer validation.</summary>
    internal const string TenantIdVariable = "FUNCTIONS_UTILS_AAD_TENANT_ID";

    /// <summary>Protected API audience for MSAL access tokens (Expose an API).</summary>
    internal const string ApiAudienceVariable = "FUNCTIONS_UTILS_AAD_AUDIENCE";

    /// <summary>SPA application (client) id; ID token audience must match when using SPA local path.</summary>
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
