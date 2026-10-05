using CleanArchitecture.Application.Common.Models;

namespace CleanArchitecture.Application.Common.Interfaces;

/// <summary>
/// Abstracts Keycloak's Admin REST API for managing users, roles, and role assignments.
/// Users and roles live entirely in Keycloak — there are no local Users/Roles tables
/// (see ADR-006 and ADR-007).
/// </summary>
public interface IIdentityAdminService
{
    /// <param name="search">Optional filter matched against username, email, first and last name.</param>
    Task<IReadOnlyList<IdentityUserDto>> GetUsersAsync(string? search, CancellationToken cancellationToken);

    /// <returns>The user, or <see langword="null"/> if no user with that ID exists.</returns>
    Task<IdentityUserDto?> GetUserAsync(string id, CancellationToken cancellationToken);

    /// <returns>The new user's ID.</returns>
    Task<string> CreateUserAsync(
        string username,
        string? email,
        string? firstName,
        string? lastName,
        string? password,
        bool temporaryPassword,
        IReadOnlyList<string> roles,
        CancellationToken cancellationToken);

    Task UpdateUserAsync(
        string id,
        string username,
        string? email,
        string? firstName,
        string? lastName,
        bool enabled,
        CancellationToken cancellationToken);

    Task DeleteUserAsync(string id, CancellationToken cancellationToken);

    Task ResetPasswordAsync(string id, string password, bool temporary, CancellationToken cancellationToken);

    /// <summary>Replaces the user's role assignments with exactly <paramref name="roleNames"/>.</summary>
    Task SetUserRolesAsync(string id, IReadOnlyList<string> roleNames, CancellationToken cancellationToken);

    /// <summary>All assignable roles, each with the permissions it grants (excludes Keycloak's built-in default roles).</summary>
    Task<IReadOnlyList<IdentityRoleDto>> GetRolesAsync(CancellationToken cancellationToken);

    /// <returns>The role, or <see langword="null"/> if no role with that name exists.</returns>
    Task<IdentityRoleDto?> GetRoleAsync(string name, CancellationToken cancellationToken);

    /// <summary>The catalog of permissions that can be granted to a role.</summary>
    Task<IReadOnlyList<IdentityPermissionDto>> GetPermissionsAsync(CancellationToken cancellationToken);

    Task CreateRoleAsync(string name, string? description, IReadOnlyList<string> permissions, CancellationToken cancellationToken);

    /// <summary>Updates the role's description and replaces its permissions with exactly <paramref name="permissions"/>.</summary>
    Task UpdateRoleAsync(string name, string? description, IReadOnlyList<string> permissions, CancellationToken cancellationToken);

    Task DeleteRoleAsync(string name, CancellationToken cancellationToken);
}
