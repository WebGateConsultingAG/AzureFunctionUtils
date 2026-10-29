using System.Security.Claims;

namespace WebGate.Azure.FunctionsUtils.Internal.Resolution;

/// <summary>Result of <see cref="ClaimsPrincipalResolver.Resolve"/> — principal plus mapped identity fields.</summary>
internal sealed class ResolvedIdentity
{
    /// <summary>Validated principal when a parser succeeded; otherwise null.</summary>
    public ClaimsPrincipal? Principal { get; init; }

    /// <summary>Object id or subject from claims; drives <see cref="IsAuthenticated"/>.</summary>
    public string? UserId { get; init; }

    /// <summary>UPN, preferred username, or email-like claim when present.</summary>
    public string? Upn { get; init; }

    /// <summary>App roles from <c>roles</c> or standard role claim types.</summary>
    public IEnumerable<string> Roles { get; init; } = [];

    /// <summary>True when <see cref="UserId"/> is non-empty (fail closed otherwise).</summary>
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(UserId);
}
