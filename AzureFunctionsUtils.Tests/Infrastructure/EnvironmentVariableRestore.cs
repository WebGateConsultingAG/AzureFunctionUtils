namespace AzureFunctionsUtils.Tests.Infrastructure;

/// <summary>Restores a process environment variable when disposed (test isolation).</summary>
internal sealed class EnvironmentVariableRestore : IDisposable
{
    private readonly string _name;
    private readonly string? _previous;

    public EnvironmentVariableRestore(string name, string? value)
    {
        _name = name;
        _previous = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Process);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(_name, _previous, EnvironmentVariableTarget.Process);
    }
}
