using Microsoft.AspNetCore.Http;

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
    protected bool _authenticated = false;
    protected bool _isDev = false;
    protected IEnumerable<string> _roles = [];

    public string? GetEnvironmentVariable(string name)
    {
        return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
    }

    public async ValueTask<T?> GetPayLoad<T>()
    {
        if (_request != null)
        {
            return await _request.ReadFromJsonAsync<T>();
        }
        else
        {
            throw new NullReferenceException();
        }
    }

    public string? GetUserId()
    {
        return _userId;
    }
    public string? GetUPN()
    {
        return _upn;
    }

    public bool IsAuthenticated()
    {
        return _authenticated;
    }

    public bool IsDev()
    {
        return _isDev;
    }

    public bool IsInAtLeastOneRole(params string[] rolesToCheck)
    {
        return _roles.Intersect(rolesToCheck).Count() > 0;
    }

    public bool IsInAllRoles(params string[] rolesToCheck)
    {
        return _roles.Intersect(rolesToCheck).Count() == rolesToCheck.Count();
    }
}