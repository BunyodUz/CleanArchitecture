using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Common.Security;
using CleanArchitecture.Domain.Constants;

namespace CleanArchitecture.Application.Users.Queries.GetUser;

[Authorize(Roles = Roles.Administrator)]
public record GetUserQuery(string Id) : IRequest<IdentityUserDto>;

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, IdentityUserDto>
{
    private readonly IIdentityAdminService _identityAdminService;

    public GetUserQueryHandler(IIdentityAdminService identityAdminService)
    {
        _identityAdminService = identityAdminService;
    }

    public async Task<IdentityUserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _identityAdminService.GetUserAsync(request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, user);

        return user;
    }
}
