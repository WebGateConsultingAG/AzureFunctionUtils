using Microsoft.AspNetCore.Http;
using WebGate.Azure.FunctionsUtils.Internal;

namespace WebGate.Azure.FunctionsUtils;

public enum FunctionRunContextType
{
    USER,
    APPLICATION
}

public abstract class FunctionRunContext(FunctionRunContextType functionRunContextType) : IFunctionRunContext
{
    public FunctionRunContextType FunctionRunContextType { get; } = functionRunContextType;
    protected HttpRequest? _request;
    protected string? _userId;
    protected string? _upn;
    protected bool _authenticated;
    protected bool _isDev;
    protected IEnumerable<string> _roles = [];

    protected static bool IsLocalDevelopmentEnvironment() => AzureFunctionsEnvironment.IsLocalDevelopment();

    public string? GetEnvironmentVariable(string name)
    {
        return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
    }

    public async ValueTask<T?> GetPayLoad<T>()
    {
        if (_request == null)
        {
            throw new InvalidOperationException("No HttpRequest is associated with this function run context.");
        }

        return await _request.ReadFromJsonAsync<T>();
    }

    public string? GetUserId() => _userId;

    public string? GetUPN() => _upn;

    public bool IsAuthenticated() => _authenticated;

    public bool IsDev() => _isDev;

    public bool IsInAtLeastOneRole(params string[] rolesToCheck)
    {
        return _roles.Intersect(rolesToCheck).Any();
    }

    public bool IsInAllRoles(params string[] rolesToCheck)
    {
        return _roles.Intersect(rolesToCheck).Count() == rolesToCheck.Length;
    }
}
