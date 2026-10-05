using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Security;
using CleanArchitecture.Domain.Constants;

namespace CleanArchitecture.Application.Users.Commands.CreateUser;

[Authorize(Permissions = Permissions.Users.Write)]
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

    /// <summary>Roles to assign on creation. Non-empty requires <see cref="Permissions.Roles.Write"/>.</summary>
    public IReadOnlyList<string> Roles { get; init; } = [];
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, string>
{
    private readonly IIdentityAdminService _identityAdminService;
    private readonly IUser _user;

    public CreateUserCommandHandler(IIdentityAdminService identityAdminService, IUser user)
    {
        _identityAdminService = identityAdminService;
        _user = user;
    }

    public Task<string> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Same rule as SetUserRolesCommand, but conditional: creating a user without roles only
        // needs users.write, so it can't be expressed with a static [Authorize] attribute.
        if (request.Roles.Count > 0 && !(_user.Permissions?.Contains(Permissions.Roles.Write) ?? false))
        {
            throw new ForbiddenAccessException();
        }

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
