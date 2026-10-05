using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Common.Security;
using CleanArchitecture.Domain.Constants;

namespace CleanArchitecture.Application.Roles.Queries.GetPermissions;

[Authorize(Permissions = Permissions.Roles.Read)]
public record GetPermissionsQuery : IRequest<IReadOnlyList<IdentityPermissionDto>>;

public class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, IReadOnlyList<IdentityPermissionDto>>
{
    private readonly IIdentityAdminService _identityAdminService;

    public GetPermissionsQueryHandler(IIdentityAdminService identityAdminService)
    {
        _identityAdminService = identityAdminService;
    }

    public Task<IReadOnlyList<IdentityPermissionDto>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        return _identityAdminService.GetPermissionsAsync(cancellationToken);
    }
}
