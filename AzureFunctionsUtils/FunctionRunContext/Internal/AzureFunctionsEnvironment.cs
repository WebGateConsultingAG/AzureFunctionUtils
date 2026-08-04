namespace WebGate.Azure.FunctionsUtils.Internal;

internal static class AzureFunctionsEnvironment
{
    private const string LOCAL_DEVELOPMENT_VARIABLE = "FUNCTIONS_UTILS_LOCAL_DEVELOPMENT";

    /// <summary>
    /// True when <c>FUNCTIONS_UTILS_LOCAL_DEVELOPMENT</c> is exactly <c>true</c> (case-insensitive).
    /// Set only in local.settings.json — never in Azure.
    /// Enables <see cref="IsDev"/> and Bearer JWT validation.
    /// </summary>
    public static bool IsLocalDevelopment()
    {
        var value = Environment.GetEnvironmentVariable(LOCAL_DEVELOPMENT_VARIABLE, EnvironmentVariableTarget.Process);
        return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }
}
