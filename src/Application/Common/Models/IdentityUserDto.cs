namespace CleanArchitecture.Application.Common.Models;

public record IdentityUserDto
{
    public string Id { get; init; } = string.Empty;

    public string? Username { get; init; }

    public string? Email { get; init; }

    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    public bool Enabled { get; init; }

    public IReadOnlyList<string> Roles { get; init; } = [];
}
