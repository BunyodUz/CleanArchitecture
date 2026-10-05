using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Security;
using CleanArchitecture.Domain.Constants;

namespace CleanArchitecture.Application.Roles.Commands.DeleteRole;

/// <summary>Deletes a role. Keycloak removes it from every user who held it.</summary>
[Authorize(Permissions = Permissions.Roles.Write)]
public record DeleteRoleCommand(string Name) : IRequest;

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand>
{
    private readonly IIdentityAdminService _identityAdminService;

    public DeleteRoleCommandHandler(IIdentityAdminService identityAdminService)
    {
        _identityAdminService = identityAdminService;
    }

    public async Task Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _identityAdminService.GetRoleAsync(request.Name, cancellationToken);

        Guard.Against.NotFound(request.Name, role);

        await _identityAdminService.DeleteRoleAsync(request.Name, cancellationToken);
    }
}
