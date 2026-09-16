using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Common.Security;
using CleanArchitecture.Domain.Constants;

namespace CleanArchitecture.Application.Users.Queries.GetUsers;

[Authorize(Roles = Roles.Administrator)]
public record GetUsersQuery(string? Search = null) : IRequest<IReadOnlyList<IdentityUserDto>>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, IReadOnlyList<IdentityUserDto>>
{
    private readonly IIdentityAdminService _identityAdminService;

    public GetUsersQueryHandler(IIdentityAdminService identityAdminService)
    {
        _identityAdminService = identityAdminService;
    }

    public Task<IReadOnlyList<IdentityUserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        return _identityAdminService.GetUsersAsync(request.Search, cancellationToken);
    }
}
