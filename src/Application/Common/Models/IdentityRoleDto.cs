namespace CleanArchitecture.Application.Common.Models;

public record IdentityRoleDto
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    /// <summary>The permissions this role grants (see <see cref="Domain.Constants.Permissions"/>).</summary>
    public IReadOnlyList<string> Permissions { get; init; } = [];
}
