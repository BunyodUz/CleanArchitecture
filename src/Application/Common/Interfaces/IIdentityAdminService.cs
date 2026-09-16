using CleanArchitecture.Application.Common.Models;

namespace CleanArchitecture.Application.Common.Interfaces;

/// <summary>
/// Abstracts Keycloak's Admin REST API for managing users and their realm role assignments.
/// Users live entirely in Keycloak — there is no local Users table (see ADR-006).
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

    /// <summary>All realm roles available to assign (excludes Keycloak's built-in default/composite roles).</summary>
    Task<IReadOnlyList<IdentityRoleDto>> GetRolesAsync(CancellationToken cancellationToken);

    /// <summary>Replaces the user's realm role assignments with exactly <paramref name="roleNames"/>.</summary>
    Task SetUserRolesAsync(string id, IReadOnlyList<string> roleNames, CancellationToken cancellationToken);
}
