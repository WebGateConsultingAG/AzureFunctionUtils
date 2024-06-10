namespace WebGate.Azure.FunctionsUtils;
public interface IFunctionRunContext {
    public FunctionRunContextType FunctionRunContextType { get; }
    public string? GetEnvironmentVariable(string name);
    public ValueTask<T?> GetPayLoad<T>();
    public string? GetUserId();
    public string? GetUPN();
    public bool IsAuthenticated();

    public bool IsDev();

    public bool IsInAtLeastOneRole(params string[] rolesToCheck);
    public bool IsInAllRoles(params string[] rolesToCheck);
}