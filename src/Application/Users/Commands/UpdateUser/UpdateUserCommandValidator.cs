using CleanArchitecture.Application.Common.Interfaces;

namespace CleanArchitecture.Application.Users.Commands.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    private readonly IIdentityAdminService _identityAdminService;

    public UpdateUserCommandValidator(IIdentityAdminService identityAdminService)
    {
        _identityAdminService = identityAdminService;

        RuleFor(v => v.Id).NotEmpty();

        RuleFor(v => v.Username)
            .NotEmpty()
            .MaximumLength(255)
            .MustAsync(BeUniqueUsername)
                .WithMessage("'{PropertyName}' is already taken.")
                .WithErrorCode("Unique");

        RuleFor(v => v.Email)
            .EmailAddress()
            .When(v => !string.IsNullOrWhiteSpace(v.Email));
    }

    private async Task<bool> BeUniqueUsername(UpdateUserCommand model, string username, CancellationToken cancellationToken)
    {
        var users = await _identityAdminService.GetUsersAsync(username, cancellationToken);

        return !users.Any(u => u.Id != model.Id && string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
    }
}
