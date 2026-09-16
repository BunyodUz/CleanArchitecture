namespace CleanArchitecture.Application.Users.Commands.ResetPassword;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();

        RuleFor(v => v.Password)
            .NotEmpty()
            .MinimumLength(8);
    }
}
