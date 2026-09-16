using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Security;
using CleanArchitecture.Domain.Constants;

namespace CleanArchitecture.Application.Users.Commands.DeleteUser;

[Authorize(Roles = Roles.Administrator)]
public record DeleteUserCommand(string Id) : IRequest;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IIdentityAdminService _identityAdminService;

    public DeleteUserCommandHandler(IIdentityAdminService identityAdminService)
    {
        _identityAdminService = identityAdminService;
    }

    public Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        return _identityAdminService.DeleteUserAsync(request.Id, cancellationToken);
    }
}
