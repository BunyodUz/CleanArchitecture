using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Security;

namespace CleanArchitecture.Application.Roles.Commands.DeleteRole;

public class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator(IIdentityAdminService identityAdminService)
    {
        RuleFor(v => v.Name)
            .NotEmpty()
            .MustAsync((name, cancellationToken) => ProtectedRoleGuard.NotBeProtectedAsync(identityAdminService, name, cancellationToken))
                .WithMessage("'{PropertyValue}' is a protected role and can't be deleted.")
                .WithErrorCode("Protected");
    }
}
