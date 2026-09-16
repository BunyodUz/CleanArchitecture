using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Security;
using CleanArchitecture.Domain.Constants;

namespace CleanArchitecture.Application.Users.Commands.ResetPassword;

[Authorize(Roles = Roles.Administrator)]
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

    public ResetPasswordCommandHandler(IIdentityAdminService identityAdminService)
    {
        _identityAdminService = identityAdminService;
    }

    public Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        return _identityAdminService.ResetPasswordAsync(request.Id, request.Password, request.Temporary, cancellationToken);
    }
}
