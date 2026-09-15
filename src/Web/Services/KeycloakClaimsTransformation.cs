using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;

namespace CleanArchitecture.Web.Services;

/// <summary>
/// Maps Keycloak's token claims onto the ClaimsPrincipal used throughout the app:
/// realm roles (<c>realm_access.roles</c>) become <see cref="ClaimTypes.Role"/> claims
/// (coarse-grained, e.g. "Administrator"), and this client's roles
/// (<c>resource_access.{clientId}.roles</c>) become <see cref="PermissionClaimType"/> claims
/// (fine-grained, e.g. "todolists.write" — see <see cref="Domain.Constants.Permissions"/>).
/// </summary>
public class KeycloakClaimsTransformation : IClaimsTransformation
{
    public const string PermissionClaimType = "permission";

    private readonly string _clientId;

    public KeycloakClaimsTransformation(IConfiguration configuration)
    {
        _clientId = configuration["Authentication:Keycloak:ClientId"]
            ?? throw new InvalidOperationException("Authentication:Keycloak:ClientId is not configured.");
    }

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
        {
            return Task.FromResult(principal);
        }

        // Already transformed (this runs on every request that authenticates the principal).
        if (identity.HasClaim(c => c.Type == PermissionClaimType) || identity.HasClaim(c => c.Type == ClaimTypes.Role))
        {
            return Task.FromResult(principal);
        }

        var realmAccessJson = principal.FindFirst("realm_access")?.Value;
        if (realmAccessJson is not null)
        {
            var realmAccess = JsonSerializer.Deserialize<RoleContainer>(realmAccessJson);
            foreach (var role in realmAccess?.Roles ?? [])
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }
        }

        var resourceAccessJson = principal.FindFirst("resource_access")?.Value;
        if (resourceAccessJson is not null)
        {
            var resourceAccess = JsonSerializer.Deserialize<Dictionary<string, RoleContainer>>(resourceAccessJson);
            if (resourceAccess is not null && resourceAccess.TryGetValue(_clientId, out var client))
            {
                foreach (var permission in client.Roles ?? [])
                {
                    identity.AddClaim(new Claim(PermissionClaimType, permission));
                }
            }
        }

        return Task.FromResult(principal);
    }

    private sealed class RoleContainer
    {
        [JsonPropertyName("roles")]
        public List<string>? Roles { get; set; }
    }
}
