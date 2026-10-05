namespace CleanArchitecture.Application.Common.Models;

public record IdentityRoleDto
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    /// <summary>The permissions this role grants (see <see cref="Domain.Constants.Permissions"/>).</summary>
    public IReadOnlyList<string> Permissions { get; init; } = [];

    /// <summary>
    /// Protected roles can't be edited or deleted, and the last enabled user holding one can't
    /// lose it. Set in Keycloak with the role attribute <c>protected = true</c> (see ADR-009), so
    /// the app never needs to know which roles a deployment has.
    /// </summary>
    public bool IsProtected { get; init; }
}
