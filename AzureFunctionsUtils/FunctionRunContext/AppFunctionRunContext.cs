namespace WebGate.Azure.FunctionsUtils;

/// <summary>
/// Application (non-user) run context with an explicit application id and role list.
/// </summary>
/// <remarks>
/// <para>
/// No HTTP headers are read. The host must ensure <paramref name="applicationId"/> and <paramref name="roles"/>
/// come from a trusted source (configuration, managed identity mapping, etc.).
/// </para>
/// <para>
/// <see cref="FunctionRunContext.IsAuthenticated"/> is true when <paramref name="applicationId"/> is non-empty.
/// </para>
/// </remarks>
public class AppFunctionRunContext : FunctionRunContext
{
    /// <summary>
    /// Creates an application context for timers, queue handlers, or tests where the caller identity is known out-of-band.
    /// </summary>
    /// <param name="applicationId">Application or service principal identifier stored as <see cref="IFunctionRunContext.GetUserId"/>.</param>
    /// <param name="roles">App roles attributed to this caller for <see cref="IFunctionRunContext.IsInAtLeastOneRole"/> checks.</param>
    public AppFunctionRunContext(string applicationId, string[] roles) : base(FunctionRunContextType.APPLICATION)
    {
        _userId = applicationId;
        _roles = roles;
        _authenticated = !string.IsNullOrWhiteSpace(applicationId);
        _isDev = IsLocalDevelopmentEnvironment();
    }
}
