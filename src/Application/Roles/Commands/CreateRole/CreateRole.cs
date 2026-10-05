using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Security;
using CleanArchitecture.Domain.Constants;

namespace CleanArchitecture.Application.Roles.Commands.CreateRole;

[Authorize(Permissions = Domain.Constants.Permissions.Roles.Write)]
public record CreateRoleCommand : IRequest
{
    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public IReadOnlyList<string> Permissions { get; init; } = [];
}

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand>
{
    private readonly IIdentityAdminService _identityAdminService;

    public CreateRoleCommandHandler(IIdentityAdminService identityAdminService)
    {
        _identityAdminService = identityAdminService;
    }

    public Task Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        return _identityAdminService.CreateRoleAsync(request.Name, request.Description, request.Permissions, cancellationToken);
    }
}
