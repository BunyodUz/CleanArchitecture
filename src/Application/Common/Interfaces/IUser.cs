using System.Security.Claims;

namespace CleanArchitecture.Application.Common.Interfaces;

public interface IUser
{
    string? Id { get; }
    string? UserName { get; }
    List<string>? Roles { get; }
    List<string>? Permissions { get; }

    /// <summary>
    /// The current caller's principal, for policy-based/resource-based authorization checks
    /// via <see cref="Microsoft.AspNetCore.Authorization.IAuthorizationService"/>. Prefer
    /// <see cref="Roles"/>/<see cref="Permissions"/> for simple checks.
    /// </summary>
    ClaimsPrincipal? Principal { get; }
}
