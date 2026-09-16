using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Security;
using CleanArchitecture.Domain.Constants;

namespace CleanArchitecture.Application.Users.Commands.SetUserRoles;

[Authorize(Roles = Domain.Constants.Roles.Administrator)]
public record SetUserRolesCommand : IRequest
{
    public string Id { get; init; } = string.Empty;

    public IReadOnlyList<string> Roles { get; init; } = [];
}

public class SetUserRolesCommandHandler : IRequestHandler<SetUserRolesCommand>
{
    private readonly IIdentityAdminService _identityAdminService;

    public SetUserRolesCommandHandler(IIdentityAdminService identityAdminService)
    {
        _identityAdminService = identityAdminService;
    }

    public Task Handle(SetUserRolesCommand request, CancellationToken cancellationToken)
    {
        return _identityAdminService.SetUserRolesAsync(request.Id, request.Roles, cancellationToken);
    }
}
