using Xunit;

namespace AzureFunctionsUtils.Tests.Infrastructure;

[CollectionDefinition(nameof(EnvironmentVariablesCollection), DisableParallelization = true)]
public class EnvironmentVariablesCollection : ICollectionFixture<object>
{
}
