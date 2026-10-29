using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using WebGate.Azure.FunctionsUtils.Internal;

namespace WebGate.Azure.FunctionsUtils;

/// <summary>
/// User identity from HTTP request headers (Easy Auth, validated Entra JWT locally).
/// </summary>
/// <remarks>
/// Use <see cref="ForRequest"/> once per HTTP request to avoid repeated JWT validation when identity is read multiple times.
/// </remarks>
public class UserFunctionRunContext : FunctionRunContext
{
    private static readonly object ContextItemKey = typeof(UserFunctionRunContext);

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

    /// <summary>
    /// Returns a single run context per HTTP request (cached in <see cref="HttpContext.Items"/> when available).
    /// </summary>
    /// <param name="request">The incoming HTTP request.</param>
    /// <returns>Resolved user run context for this request.</returns>
    public static UserFunctionRunContext ForRequest(HttpRequest request)
    {
        var items = request.HttpContext?.Items;
        if (items != null &&
            items.TryGetValue(ContextItemKey, out var existing) &&
            existing is UserFunctionRunContext cached)
        {
            return cached;
        }

        var context = new UserFunctionRunContext(request);
        if (items != null)
        {
            items[ContextItemKey] = context;
        }

        return context;
    }

    public ClaimsPrincipal? GetClaimsPrincipal() => _principal;
}
