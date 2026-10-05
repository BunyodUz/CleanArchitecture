using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Security;

namespace CleanArchitecture.Application.Roles.Commands.UpdateRole;

public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    private readonly IIdentityAdminService _identityAdminService;

    public UpdateRoleCommandValidator(IIdentityAdminService identityAdminService)
    {
        _identityAdminService = identityAdminService;

        RuleFor(v => v.Name)
            .NotEmpty()
            // A protected role is how administrators keep roles.write; letting it be edited would
            // let the last admin strip their own ability to fix it.
            .MustAsync((name, cancellationToken) => ProtectedRoleGuard.NotBeProtectedAsync(_identityAdminService, name, cancellationToken))
                .WithMessage("'{PropertyValue}' is a protected role and can't be modified.")
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
