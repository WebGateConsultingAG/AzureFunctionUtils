namespace WebGate.Azure.FunctionsUtils;

/// <summary>
/// Distinguishes user HTTP contexts from explicit application identities.
/// </summary>
public enum FunctionRunContextType
{
    /// <summary>End-user identity from <see cref="UserFunctionRunContext"/>.</summary>
    USER,

    /// <summary>Non-interactive application identity from <see cref="AppFunctionRunContext"/>.</summary>
    APPLICATION
}
