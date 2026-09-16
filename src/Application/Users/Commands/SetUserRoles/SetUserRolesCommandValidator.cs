using CleanArchitecture.Application.Common.Interfaces;

namespace CleanArchitecture.Application.Users.Commands.SetUserRoles;

public class SetUserRolesCommandValidator : AbstractValidator<SetUserRolesCommand>
{
    private readonly IIdentityAdminService _identityAdminService;

    public SetUserRolesCommandValidator(IIdentityAdminService identityAdminService)
    {
        _identityAdminService = identityAdminService;

        RuleFor(v => v.Id).NotEmpty();

        RuleFor(v => v.Roles)
            .MustAsync(BeKnownRoles)
                .WithMessage("One or more roles do not exist.")
                .WithErrorCode("Unknown");
    }

    private async Task<bool> BeKnownRoles(IReadOnlyList<string> roleNames, CancellationToken cancellationToken)
    {
        if (roleNames.Count == 0) return true;

        var availableRoles = await _identityAdminService.GetRolesAsync(cancellationToken);
        var availableRoleNames = availableRoles.Select(r => r.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        return roleNames.All(availableRoleNames.Contains);
    }
}
