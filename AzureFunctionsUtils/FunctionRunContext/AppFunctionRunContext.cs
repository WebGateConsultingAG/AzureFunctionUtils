namespace WebGate.Azure.FunctionsUtils;

public class AppFunctionRunContext : FunctionRunContext
{
    public AppFunctionRunContext(string applicationId, string[] roles) : base(FunctionRunContextType.APPLICATION)
    {
        _userId = applicationId;
        _roles = roles;
        _authenticated = !string.IsNullOrWhiteSpace(applicationId);
        _isDev = IsLocalDevelopmentEnvironment();
    }
}
