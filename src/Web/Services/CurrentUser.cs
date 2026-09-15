using System.Security.Claims;

using CleanArchitecture.Application.Common.Interfaces;

namespace CleanArchitecture.Web.Services;

public class CurrentUser : IUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    // "sub" is the Keycloak user id. Fall back to NameIdentifier in case inbound claim
    // mapping remaps it — see the OpenIdConnect handler's MapInboundClaims behaviour.
    public string? Id => User?.FindFirstValue("sub") ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? UserName => User?.FindFirstValue("preferred_username") ?? User?.Identity?.Name;

    public List<string>? Roles => User?.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList();

    public List<string>? Permissions => User?.FindAll(KeycloakClaimsTransformation.PermissionClaimType).Select(x => x.Value).ToList();

    public ClaimsPrincipal? Principal => User;
}
