using CleanArchitecture.Application.Common.Interfaces;

namespace CleanArchitecture.Application.Users.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    private readonly IIdentityAdminService _identityAdminService;

    public CreateUserCommandValidator(IIdentityAdminService identityAdminService)
    {
        _identityAdminService = identityAdminService;

        RuleFor(v => v.Username)
            .NotEmpty()
            .MaximumLength(255)
            .MustAsync(BeUniqueUsername)
                .WithMessage("'{PropertyName}' is already taken.")
                .WithErrorCode("Unique");

        RuleFor(v => v.Email)
            .EmailAddress()
            .When(v => !string.IsNullOrWhiteSpace(v.Email));

        RuleFor(v => v.Password)
            .MinimumLength(8)
            .When(v => !string.IsNullOrWhiteSpace(v.Password));

        RuleForEach(v => v.Roles).NotEmpty();
    }

    private async Task<bool> BeUniqueUsername(string username, CancellationToken cancellationToken)
    {
        var users = await _identityAdminService.GetUsersAsync(username, cancellationToken);

        return !users.Any(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
    }
}
