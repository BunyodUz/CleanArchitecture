using CleanArchitecture.Application.AuditEntries;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Security;
using CleanArchitecture.Domain.Constants;

namespace CleanArchitecture.Application.Users.Commands.UpdateUser;

[Authorize(Permissions = Permissions.Users.Write)]
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
    private readonly IAuditLog _auditLog;

    public UpdateUserCommandHandler(IIdentityAdminService identityAdminService, IAuditLog auditLog)
    {
        _identityAdminService = identityAdminService;
        _auditLog = auditLog;
    }

    public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var before = await _identityAdminService.GetUserAsync(request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, before);

        await _identityAdminService.UpdateUserAsync(
            request.Id,
            request.Username,
            request.Email,
            request.FirstName,
            request.LastName,
            request.Enabled,
            cancellationToken);

        await _auditLog.RecordAsync(
            AuditActions.UserUpdated,
            AuditTargets.User,
            request.Id,
            request.Username,
            AuditDetails.Changes(
                ("username", before.Username, request.Username),
                ("email", before.Email, request.Email),
                ("first name", before.FirstName, request.FirstName),
                ("last name", before.LastName, request.LastName),
                ("status", Status(before.Enabled), Status(request.Enabled))),
            cancellationToken);
    }

    private static string Status(bool enabled) => enabled ? "enabled" : "disabled";
}
