namespace WebGate.Azure.FunctionsUtils;

/// <summary>
/// Shared contract for reading caller identity, roles, optional JSON body, and process environment in Azure Functions.
/// </summary>
/// <remarks>
/// <para>
/// Authorization is the host app's responsibility: use <see cref="IsAuthenticated"/> and role checks;
/// do not treat <see cref="IsDev"/> alone as proof of identity.
/// </para>
/// <para>
/// <see cref="GetPayLoad{T}"/> deserializes the HTTP body only; it performs no authentication.
/// </para>
/// </remarks>
public interface IFunctionRunContext
{
    /// <summary>Whether this context represents an end user (<see cref="FunctionRunContextType.USER"/>) or an application caller.</summary>
    public FunctionRunContextType FunctionRunContextType { get; }

    /// <summary>Reads a process-scoped environment variable (same scope as Azure Functions host configuration).</summary>
    public string? GetEnvironmentVariable(string name);

    /// <summary>Deserializes the associated HTTP request JSON body. Requires a user context bound to an <see cref="Microsoft.AspNetCore.Http.HttpRequest"/>.</summary>
    public ValueTask<T?> GetPayLoad<T>();

    /// <summary>Entra object id or equivalent resolved from validated Easy Auth or local JWT claims; null when unauthenticated.</summary>
    public string? GetUserId();

    /// <summary>User principal name or email-like identifier from claims; may be null even when <see cref="GetUserId"/> is set.</summary>
    public string? GetUPN();

    /// <summary>True when a non-empty user id was resolved (fail closed when identity is missing or invalid).</summary>
    public bool IsAuthenticated();

    /// <summary>True when <c>FUNCTIONS_UTILS_LOCAL_DEVELOPMENT</c> is enabled — not a substitute for authentication.</summary>
    public bool IsDev();

    /// <summary>True when the caller has at least one of the given app roles from the resolved principal.</summary>
    public bool IsInAtLeastOneRole(params string[] rolesToCheck);

    /// <summary>True when the caller has all of the given app roles from the resolved principal.</summary>
    public bool IsInAllRoles(params string[] rolesToCheck);
}
