namespace WebGate.Azure.FunctionsUtils.Internal;

internal static class AzureFunctionsEnvironment
{
    private const string AZURE_FUNCTIONS_ENVIRONMENT_VARIABLE = "AZURE_FUNCTIONS_ENVIRONMENT";
    private const string LOCAL_DEVELOPMENT_ENVIRONMENT_NAME = "LocalDevelopment";

    /// <summary>
    /// True when functions run on a developer machine.
    /// Azure cloud environments use other values (e.g. Development, Staging, Production).
    /// </summary>
    public static bool IsLocalDevelopment()
    {
        return string.Equals(
            Environment.GetEnvironmentVariable(AZURE_FUNCTIONS_ENVIRONMENT_VARIABLE, EnvironmentVariableTarget.Process),
            LOCAL_DEVELOPMENT_ENVIRONMENT_NAME,
            StringComparison.OrdinalIgnoreCase);
    }
}
