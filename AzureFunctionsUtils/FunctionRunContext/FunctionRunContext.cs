using Microsoft.AspNetCore.Http;
using WebGate.Azure.FunctionsUtils.Internal.Environment;

namespace WebGate.Azure.FunctionsUtils;

/// <summary>
/// Base implementation of <see cref="IFunctionRunContext"/> for user and application callers.
/// </summary>
/// <remarks>
/// User identity is populated only through <see cref="UserFunctionRunContext"/> (Easy Auth and, in local dev, validated JWT).
/// Application identity is supplied explicitly by the host; this library does not validate non-HTTP callers.
/// </remarks>
public abstract class FunctionRunContext(FunctionRunContextType functionRunContextType) : IFunctionRunContext
{
    /// <inheritdoc />
    public FunctionRunContextType FunctionRunContextType { get; } = functionRunContextType;
    protected HttpRequest? _request;
    protected string? _userId;
    protected string? _upn;
    protected bool _authenticated;
    protected bool _isDev;
    protected IEnumerable<string> _roles = [];

    protected static bool IsLocalDevelopmentEnvironment() => AzureFunctionsEnvironment.IsLocalDevelopment();

    /// <inheritdoc />
    public string? GetEnvironmentVariable(string name)
    {
        return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">When this context has no associated HTTP request (e.g. <see cref="AppFunctionRunContext"/>).</exception>
    public async ValueTask<T?> GetPayLoad<T>()
    {
        if (_request == null)
        {
            throw new InvalidOperationException("No HttpRequest is associated with this function run context.");
        }

        return await _request.ReadFromJsonAsync<T>();
    }

    /// <inheritdoc />
    public string? GetUserId() => _userId;

    /// <inheritdoc />
    public string? GetUPN() => _upn;

    /// <inheritdoc />
    public bool IsAuthenticated() => _authenticated;

    /// <inheritdoc />
    public bool IsDev() => _isDev;

    /// <inheritdoc />
    public bool IsInAtLeastOneRole(params string[] rolesToCheck)
    {
        return _roles.Intersect(rolesToCheck).Any();
    }

    /// <inheritdoc />
    public bool IsInAllRoles(params string[] rolesToCheck)
    {
        return _roles.Intersect(rolesToCheck).Count() == rolesToCheck.Length;
    }
}
