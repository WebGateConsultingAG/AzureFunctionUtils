using Microsoft.AspNetCore.Http;
using WebGate.Azure.FunctionsUtils;
using Xunit;

namespace AzureFunctionsUtils.Tests.FunctionRunContext;

public class UserFunctionRunContextTests
{
    [Fact]
    public void ForRequest_returnsSameInstanceFromHttpContextItems()
    {
        var context = new DefaultHttpContext();

        var first = UserFunctionRunContext.ForRequest(context.Request);
        var second = UserFunctionRunContext.ForRequest(context.Request);

        Assert.Same(first, second);
    }
}
