using CleanArchitecture.Application.AuditEntries;
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
    private readonly IAuditLog _auditLog;

    public CreateRoleCommandHandler(IIdentityAdminService identityAdminService, IAuditLog auditLog)
    {
        _identityAdminService = identityAdminService;
        _auditLog = auditLog;
    }

    public async Task Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        await _identityAdminService.CreateRoleAsync(request.Name, request.Description, request.Permissions, cancellationToken);

        await _auditLog.RecordAsync(
            AuditActions.RoleCreated,
            AuditTargets.Role,
            request.Name,
            request.Name,
            AuditDetails.SetChanges("permissions", [], request.Permissions),
            cancellationToken);
    }
}
