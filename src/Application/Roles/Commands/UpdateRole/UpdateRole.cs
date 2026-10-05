using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Security;
using CleanArchitecture.Domain.Constants;

namespace CleanArchitecture.Application.Roles.Commands.UpdateRole;

/// <summary>Updates a role's description and replaces its permissions. Role names are immutable.</summary>
[Authorize(Permissions = Domain.Constants.Permissions.Roles.Write)]
public record UpdateRoleCommand : IRequest
{
    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public IReadOnlyList<string> Permissions { get; init; } = [];
}

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand>
{
    private readonly IIdentityAdminService _identityAdminService;

    public UpdateRoleCommandHandler(IIdentityAdminService identityAdminService)
    {
        _identityAdminService = identityAdminService;
    }

    public async Task Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _identityAdminService.GetRoleAsync(request.Name, cancellationToken);

        Guard.Against.NotFound(request.Name, role);

        await _identityAdminService.UpdateRoleAsync(request.Name, request.Description, request.Permissions, cancellationToken);
    }
}
