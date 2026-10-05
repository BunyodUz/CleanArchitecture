namespace CleanArchitecture.Application.Roles.Commands.DeleteRole;

public class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty()
            .Must(name => !string.Equals(name, Domain.Constants.Roles.Administrator, StringComparison.OrdinalIgnoreCase))
                .WithMessage("The Administrator role is built in and can't be deleted.")
                .WithErrorCode("Protected");
    }
}
