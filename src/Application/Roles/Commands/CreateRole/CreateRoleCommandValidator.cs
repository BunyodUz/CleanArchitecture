using CleanArchitecture.Application.Common.Interfaces;

namespace CleanArchitecture.Application.Roles.Commands.CreateRole;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    private readonly IIdentityAdminService _identityAdminService;

    public CreateRoleCommandValidator(IIdentityAdminService identityAdminService)
    {
        _identityAdminService = identityAdminService;

        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength(100)
            .Matches("^[A-Za-z][A-Za-z0-9 _-]*$")
                .WithMessage("'{PropertyName}' must start with a letter and contain only letters, digits, spaces, '-' or '_'.")
            .Must(name => name is null || !name.StartsWith("default-roles-", StringComparison.OrdinalIgnoreCase))
                .WithMessage("'{PropertyName}' is reserved.")
            .MustAsync(BeUniqueName)
                .WithMessage("'{PropertyName}' is already taken.")
                .WithErrorCode("Unique");

        RuleFor(v => v.Description).MaximumLength(255);

        RuleFor(v => v.Permissions)
            .MustAsync(BeKnownPermissions)
                .WithMessage("One or more permissions do not exist.")
                .WithErrorCode("Unknown");
    }

    // Case-insensitive on purpose: "administrator" next to "Administrator" is a trap, not a new role.
    private async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
    {
        var roles = await _identityAdminService.GetRolesAsync(cancellationToken);

        return !roles.Any(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<bool> BeKnownPermissions(IReadOnlyList<string> permissions, CancellationToken cancellationToken)
    {
        if (permissions.Count == 0) return true;

        var catalog = await _identityAdminService.GetPermissionsAsync(cancellationToken);
        var known = catalog.Select(p => p.Name).ToHashSet(StringComparer.Ordinal);

        return permissions.All(known.Contains);
    }
}
