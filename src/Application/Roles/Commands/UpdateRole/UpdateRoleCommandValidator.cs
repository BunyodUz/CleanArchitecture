using CleanArchitecture.Application.Common.Interfaces;

namespace CleanArchitecture.Application.Roles.Commands.UpdateRole;

public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    private readonly IIdentityAdminService _identityAdminService;

    public UpdateRoleCommandValidator(IIdentityAdminService identityAdminService)
    {
        _identityAdminService = identityAdminService;

        RuleFor(v => v.Name)
            .NotEmpty()
            // Administrator holds roles.write; letting it be edited would let the last admin
            // strip their own ability to fix it.
            .Must(name => !string.Equals(name, Domain.Constants.Roles.Administrator, StringComparison.OrdinalIgnoreCase))
                .WithMessage("The Administrator role is built in and can't be modified.")
                .WithErrorCode("Protected");

        RuleFor(v => v.Description).MaximumLength(255);

        RuleFor(v => v.Permissions)
            .MustAsync(BeKnownPermissions)
                .WithMessage("One or more permissions do not exist.")
                .WithErrorCode("Unknown");
    }

    private async Task<bool> BeKnownPermissions(IReadOnlyList<string> permissions, CancellationToken cancellationToken)
    {
        if (permissions.Count == 0) return true;

        var catalog = await _identityAdminService.GetPermissionsAsync(cancellationToken);
        var known = catalog.Select(p => p.Name).ToHashSet(StringComparer.Ordinal);

        return permissions.All(known.Contains);
    }
}
