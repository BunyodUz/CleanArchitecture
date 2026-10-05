using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Security;
using CleanArchitecture.Domain.Constants;

namespace CleanArchitecture.Application.Users.Commands.DeleteUser;

[Authorize(Permissions = Permissions.Users.Write)]
public record DeleteUserCommand(string Id) : IRequest;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IIdentityAdminService _identityAdminService;
    private readonly IAuditLog _auditLog;

    public DeleteUserCommandHandler(IIdentityAdminService identityAdminService, IAuditLog auditLog)
    {
        _identityAdminService = identityAdminService;
        _auditLog = auditLog;
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        // Looked up first so the audit entry can name the user once they're gone.
        var user = await _identityAdminService.GetUserAsync(request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, user);

        if (user.Enabled && user.Roles.Count > 0)
        {
            var protectedRoles = await ProtectedRoleGuard.GetProtectedRoleNamesAsync(_identityAdminService, cancellationToken);
            if (ProtectedRoleGuard.CountsAsAdministrator(user, protectedRoles))
            {
                await ProtectedRoleGuard.EnsureAnotherAdministratorAsync(
                    _identityAdminService, protectedRoles, request.Id, nameof(request.Id), cancellationToken);
            }
        }

        await _identityAdminService.DeleteUserAsync(request.Id, cancellationToken);

        await _auditLog.RecordAsync(AuditActions.UserDeleted, AuditTargets.User, request.Id, user.Username, null, cancellationToken);
    }
}
