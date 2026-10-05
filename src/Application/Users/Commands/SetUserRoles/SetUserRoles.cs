using CleanArchitecture.Application.AuditEntries;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Security;
using CleanArchitecture.Domain.Constants;

namespace CleanArchitecture.Application.Users.Commands.SetUserRoles;

// Assigning roles is how permissions are granted, so it needs roles.write on top of users.write —
// otherwise users.write alone would be enough to promote anyone (including yourself) to Administrator.
[Authorize(Permissions = Permissions.Users.Write + "," + Permissions.Roles.Write)]
public record SetUserRolesCommand : IRequest
{
    public string Id { get; init; } = string.Empty;

    public IReadOnlyList<string> Roles { get; init; } = [];
}

public class SetUserRolesCommandHandler : IRequestHandler<SetUserRolesCommand>
{
    private readonly IIdentityAdminService _identityAdminService;
    private readonly IAuditLog _auditLog;

    public SetUserRolesCommandHandler(IIdentityAdminService identityAdminService, IAuditLog auditLog)
    {
        _identityAdminService = identityAdminService;
        _auditLog = auditLog;
    }

    public async Task Handle(SetUserRolesCommand request, CancellationToken cancellationToken)
    {
        var before = await _identityAdminService.GetUserAsync(request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, before);

        // Only removing roles can take away administrator access.
        if (before.Roles.Except(request.Roles, StringComparer.Ordinal).Any())
        {
            var protectedRoles = await ProtectedRoleGuard.GetProtectedRoleNamesAsync(_identityAdminService, cancellationToken);
            if (ProtectedRoleGuard.CountsAsAdministrator(before, protectedRoles) && !request.Roles.Any(protectedRoles.Contains))
            {
                await ProtectedRoleGuard.EnsureAnotherAdministratorAsync(
                    _identityAdminService, protectedRoles, request.Id, nameof(request.Roles), cancellationToken);
            }
        }

        await _identityAdminService.SetUserRolesAsync(request.Id, request.Roles, cancellationToken);

        // Saving the user dialog always sends the role list; only record an actual change.
        var details = AuditDetails.SetChanges("roles", before.Roles, request.Roles);
        if (details is not null)
        {
            await _auditLog.RecordAsync(AuditActions.UserRolesChanged, AuditTargets.User, request.Id, before.Username, details, cancellationToken);
        }
    }
}
