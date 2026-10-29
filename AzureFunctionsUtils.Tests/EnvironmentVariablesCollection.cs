using Xunit;

namespace AzureFunctionsUtils.Tests;

[CollectionDefinition(nameof(EnvironmentVariablesCollection), DisableParallelization = true)]
public class EnvironmentVariablesCollection : ICollectionFixture<object>
{
}
