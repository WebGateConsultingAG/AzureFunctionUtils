using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using WebGate.Azure.FunctionsUtils.Internal;

namespace WebGate.Azure.FunctionsUtils;

public class UserFunctionRunContext : FunctionRunContext
{
    private readonly ClaimsPrincipal? _principal;

    public UserFunctionRunContext(HttpRequest request) : base(FunctionRunContextType.USER)
    {
        _request = request;
        _isDev = IsLocalDevelopmentEnvironment();

        var identity = ClaimsPrincipalResolver.Resolve(request);
        _principal = identity.Principal;
        _userId = identity.UserId;
        _upn = identity.Upn;
        _roles = identity.Roles;
        _authenticated = identity.IsAuthenticated;
    }

    public ClaimsPrincipal? GetClaimsPrincipal() => _principal;
}
