using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Security;
using CleanArchitecture.Domain.Constants;

namespace CleanArchitecture.Application.Users.Commands.CreateUser;

[Authorize(Roles = Domain.Constants.Roles.Administrator)]
public record CreateUserCommand : IRequest<string>
{
    public string Username { get; init; } = string.Empty;

    public string? Email { get; init; }

    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    /// <summary>Optional initial password. When omitted, Keycloak requires the user to set one via
    /// a "forgot password" flow before they can log in.</summary>
    public string? Password { get; init; }

    /// <summary>When <see langword="true"/> (the default), the user must change the password on first login.</summary>
    public bool TemporaryPassword { get; init; } = true;

    public IReadOnlyList<string> Roles { get; init; } = [];
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, string>
{
    private readonly IIdentityAdminService _identityAdminService;

    public CreateUserCommandHandler(IIdentityAdminService identityAdminService)
    {
        _identityAdminService = identityAdminService;
    }

    public Task<string> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        return _identityAdminService.CreateUserAsync(
            request.Username,
            request.Email,
            request.FirstName,
            request.LastName,
            request.Password,
            request.TemporaryPassword,
            request.Roles,
            cancellationToken);
    }
}
