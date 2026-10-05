using CleanArchitecture.Application.AuditEntries;
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
    private readonly IAuditLog _auditLog;

    public UpdateRoleCommandHandler(IIdentityAdminService identityAdminService, IAuditLog auditLog)
    {
        _identityAdminService = identityAdminService;
        _auditLog = auditLog;
    }

    public async Task Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _identityAdminService.GetRoleAsync(request.Name, cancellationToken);

        Guard.Against.NotFound(request.Name, role);

        await _identityAdminService.UpdateRoleAsync(request.Name, request.Description, request.Permissions, cancellationToken);

        var details = AuditDetails.Join(
            AuditDetails.Changes(("description", role.Description, request.Description)),
            AuditDetails.SetChanges("permissions", role.Permissions, request.Permissions));
        if (details is not null)
        {
            await _auditLog.RecordAsync(AuditActions.RoleUpdated, AuditTargets.Role, request.Name, request.Name, details, cancellationToken);
        }
    }
}
