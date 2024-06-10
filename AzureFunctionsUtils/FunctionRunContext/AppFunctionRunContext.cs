namespace WebGate.Azure.FunctionsUtils;

public class AppFunctionRunContext:FunctionRunContext {
    
    public AppFunctionRunContext(string applicationId, string[] roles):base(FunctionRunContextType.APPLICATION) {
        _userId = applicationId;
        _roles = roles;
        string? env = GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");
        bool isDevSet = string.IsNullOrEmpty(GetEnvironmentVariable("IS_NOT_DEV"));

        if (env == "Development" && isDevSet)
        {
            _isDev = true;
        }
    }
}