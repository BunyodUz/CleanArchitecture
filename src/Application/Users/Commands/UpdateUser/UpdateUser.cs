using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Security;
using CleanArchitecture.Domain.Constants;

namespace CleanArchitecture.Application.Users.Commands.UpdateUser;

[Authorize(Roles = Roles.Administrator)]
public record UpdateUserCommand : IRequest
{
    public string Id { get; init; } = string.Empty;

    public string Username { get; init; } = string.Empty;

    public string? Email { get; init; }

    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    public bool Enabled { get; init; } = true;
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
{
    private readonly IIdentityAdminService _identityAdminService;

    public UpdateUserCommandHandler(IIdentityAdminService identityAdminService)
    {
        _identityAdminService = identityAdminService;
    }

    public Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        return _identityAdminService.UpdateUserAsync(
            request.Id,
            request.Username,
            request.Email,
            request.FirstName,
            request.LastName,
            request.Enabled,
            cancellationToken);
    }
}
