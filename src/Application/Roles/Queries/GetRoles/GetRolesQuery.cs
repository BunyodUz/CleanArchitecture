using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Common.Security;
using CleanArchitecture.Domain.Constants;

namespace CleanArchitecture.Application.Roles.Queries.GetRoles;

[Authorize(Permissions = Permissions.Roles.Read)]
public record GetRolesQuery : IRequest<IReadOnlyList<IdentityRoleDto>>;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, IReadOnlyList<IdentityRoleDto>>
{
    private readonly IIdentityAdminService _identityAdminService;

    public GetRolesQueryHandler(IIdentityAdminService identityAdminService)
    {
        _identityAdminService = identityAdminService;
    }

    public Task<IReadOnlyList<IdentityRoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        return _identityAdminService.GetRolesAsync(cancellationToken);
    }
}
