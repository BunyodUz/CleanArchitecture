using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Security;
using CleanArchitecture.Domain.Constants;

namespace CleanArchitecture.Application.Users.Commands.ResetPassword;

[Authorize(Permissions = Permissions.Users.Write)]
public record ResetPasswordCommand : IRequest
{
    public string Id { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    /// <summary>When <see langword="true"/> (the default), the user must change the password on next login.</summary>
    public bool Temporary { get; init; } = true;
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand>
{
    private readonly IIdentityAdminService _identityAdminService;
    private readonly IAuditLog _auditLog;

    public ResetPasswordCommandHandler(IIdentityAdminService identityAdminService, IAuditLog auditLog)
    {
        _identityAdminService = identityAdminService;
        _auditLog = auditLog;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityAdminService.GetUserAsync(request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, user);

        await _identityAdminService.ResetPasswordAsync(request.Id, request.Password, request.Temporary, cancellationToken);

        // Never record the password itself.
        await _auditLog.RecordAsync(
            AuditActions.UserPasswordReset,
            AuditTargets.User,
            request.Id,
            user.Username,
            request.Temporary ? "must change at next sign-in" : null,
            cancellationToken);
    }
}
