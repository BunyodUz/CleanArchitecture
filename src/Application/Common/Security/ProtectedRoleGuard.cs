using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using FluentValidation.Results;
using ValidationException = CleanArchitecture.Application.Common.Exceptions.ValidationException;

namespace CleanArchitecture.Application.Common.Security;

/// <summary>
/// Keeps the admin area reachable. Which roles are protected is data in Keycloak (the role
/// attribute <c>protected = true</c>, see ADR-009), so nothing here knows any role by name.
/// Protected roles can't be edited or deleted, and a change to a user may not leave the realm
/// without an enabled user holding a protected role.
/// </summary>
public static class ProtectedRoleGuard
{
    /// <summary>Validator helper: <see langword="true"/> unless <paramref name="roleName"/> names a protected role.</summary>
    public static async Task<bool> NotBeProtectedAsync(
        IIdentityAdminService identityAdminService, string roleName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(roleName)) return true;

        var role = await identityAdminService.GetRoleAsync(roleName, cancellationToken);

        return role is not { IsProtected: true };
    }

    public static async Task<IReadOnlySet<string>> GetProtectedRoleNamesAsync(
        IIdentityAdminService identityAdminService, CancellationToken cancellationToken)
    {
        var roles = await identityAdminService.GetRolesAsync(cancellationToken);

        return roles.Where(r => r.IsProtected).Select(r => r.Name).ToHashSet(StringComparer.Ordinal);
    }

    /// <summary>Whether the user currently counts as an administrator: enabled and holding a protected role.</summary>
    public static bool CountsAsAdministrator(IdentityUserDto user, IReadOnlySet<string> protectedRoles) =>
        user.Enabled && user.Roles.Any(protectedRoles.Contains);

    /// <summary>
    /// Throws a <see cref="ValidationException"/> on <paramref name="propertyName"/> unless some
    /// enabled user other than <paramref name="userId"/> holds a protected role. Call it before a
    /// change that stops <paramref name="userId"/> counting as an administrator.
    /// </summary>
    public static async Task EnsureAnotherAdministratorAsync(
        IIdentityAdminService identityAdminService,
        IReadOnlySet<string> protectedRoles,
        string userId,
        string propertyName,
        CancellationToken cancellationToken)
    {
        foreach (var role in protectedRoles)
        {
            var members = await identityAdminService.GetRoleMembersAsync(role, cancellationToken);
            if (members.Any(m => m.Enabled && m.Id != userId)) return;
        }

        var roleList = string.Join(", ", protectedRoles.Order(StringComparer.Ordinal));
        throw new ValidationException(
        [
            new ValidationFailure(
                propertyName,
                $"This is the last enabled user with a protected role ({roleList}). Give that role to another user first, " +
                "or nobody will be able to manage users and roles."),
        ]);
    }
}
