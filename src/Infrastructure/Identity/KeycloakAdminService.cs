using System.Net;
using System.Net.Http.Json;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using FluentValidation.Results;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Infrastructure.Identity;

/// <summary>
/// Implements <see cref="IIdentityAdminService"/> against Keycloak's Admin REST API. There are no
/// local Users/Roles tables — every operation here is a direct HTTP call to Keycloak.
/// <para>
/// RBAC model (ADR-007): roles are realm roles; permissions are client roles of the web client;
/// a role grants permissions by being a composite of them. Users are only ever assigned roles.
/// </para>
/// </summary>
public class KeycloakAdminService : IIdentityAdminService
{
    // Keycloak gives every realm a synthetic composite role ("default-roles-{realm}") that it
    // assigns to every user itself. It's not an RBAC role an admin manages, so it's hidden from
    // every role listing and left untouched when a user's roles are replaced.
    private const string DefaultRolesPrefix = "default-roles-";

    private readonly HttpClient _httpClient;
    private readonly string _realm;
    private readonly string _permissionsClientId;

    public KeycloakAdminService(HttpClient httpClient, IOptions<KeycloakAdminOptions> options)
    {
        _httpClient = httpClient;
        _realm = options.Value.Realm;
        _permissionsClientId = options.Value.PermissionsClientId;
    }

    // ── Users ────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<IdentityUserDto>> GetUsersAsync(string? search, CancellationToken cancellationToken)
    {
        var query = string.IsNullOrWhiteSpace(search) ? string.Empty : $"?search={Uri.EscapeDataString(search)}";

        var users = await _httpClient.GetFromJsonAsync<List<UserRepresentation>>(
            $"admin/realms/{_realm}/users{query}", cancellationToken) ?? [];

        var result = new List<IdentityUserDto>(users.Count);
        foreach (var user in users)
        {
            result.Add(await ToUserDtoAsync(user, cancellationToken));
        }

        return result;
    }

    public async Task<IdentityUserDto?> GetUserAsync(string id, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"admin/realms/{_realm}/users/{id}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;

        response.EnsureSuccessStatusCode();

        var user = await response.Content.ReadFromJsonAsync<UserRepresentation>(cancellationToken);

        return user is null ? null : await ToUserDtoAsync(user, cancellationToken);
    }

    public async Task<string> CreateUserAsync(
        string username,
        string? email,
        string? firstName,
        string? lastName,
        string? password,
        bool temporaryPassword,
        IReadOnlyList<string> roles,
        CancellationToken cancellationToken)
    {
        var representation = new UserRepresentation
        {
            Username = username,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Enabled = true,
            EmailVerified = true,
            Credentials = string.IsNullOrEmpty(password)
                ? null
                : [new CredentialRepresentation { Type = "password", Value = password, Temporary = temporaryPassword }],
        };

        var response = await _httpClient.PostAsJsonAsync($"admin/realms/{_realm}/users", representation, cancellationToken);
        EnsureSuccess(response, nameof(username));

        // Keycloak's create-user response has no body — the new user's ID is the last segment
        // of the Location header instead.
        var location = response.Headers.Location
            ?? throw new InvalidOperationException("Keycloak did not return a Location header for the created user.");
        var id = location.Segments[^1];

        if (roles.Count > 0)
        {
            await SetUserRolesAsync(id, roles, cancellationToken);
        }

        return id;
    }

    public async Task UpdateUserAsync(
        string id,
        string username,
        string? email,
        string? firstName,
        string? lastName,
        bool enabled,
        CancellationToken cancellationToken)
    {
        var representation = new UserRepresentation
        {
            Username = username,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Enabled = enabled,
        };

        var response = await _httpClient.PutAsJsonAsync($"admin/realms/{_realm}/users/{id}", representation, cancellationToken);
        EnsureSuccess(response, nameof(username));
    }

    public async Task DeleteUserAsync(string id, CancellationToken cancellationToken)
    {
        var response = await _httpClient.DeleteAsync($"admin/realms/{_realm}/users/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task ResetPasswordAsync(string id, string password, bool temporary, CancellationToken cancellationToken)
    {
        var credential = new CredentialRepresentation { Type = "password", Value = password, Temporary = temporary };

        var response = await _httpClient.PutAsJsonAsync(
            $"admin/realms/{_realm}/users/{id}/reset-password", credential, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task SetUserRolesAsync(string id, IReadOnlyList<string> roleNames, CancellationToken cancellationToken)
    {
        var allRoles = await GetAssignableRealmRolesAsync(cancellationToken);
        var targetRoles = allRoles.Where(r => roleNames.Contains(r.Name!, StringComparer.OrdinalIgnoreCase)).ToList();

        var currentRoles = await GetUserRoleRepresentationsAsync(id, cancellationToken);

        await ReplaceMappingsAsync(
            $"admin/realms/{_realm}/users/{id}/role-mappings/realm", currentRoles, targetRoles, cancellationToken);
    }

    // ── Roles ────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<IdentityRoleDto>> GetRolesAsync(CancellationToken cancellationToken)
    {
        var clientUuid = await GetPermissionsClientUuidAsync(cancellationToken);
        var roles = await GetAssignableRealmRolesAsync(cancellationToken);

        var result = new List<IdentityRoleDto>(roles.Count);
        foreach (var role in roles.OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase))
        {
            result.Add(await ToRoleDtoAsync(role, clientUuid, cancellationToken));
        }

        return result;
    }

    public async Task<IdentityRoleDto?> GetRoleAsync(string name, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(RolePath(name), cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;

        response.EnsureSuccessStatusCode();

        var role = await response.Content.ReadFromJsonAsync<RoleRepresentation>(cancellationToken);
        if (role is null || !IsAssignable(role)) return null;

        var clientUuid = await GetPermissionsClientUuidAsync(cancellationToken);

        return await ToRoleDtoAsync(role, clientUuid, cancellationToken);
    }

    public async Task<IReadOnlyList<IdentityPermissionDto>> GetPermissionsAsync(CancellationToken cancellationToken)
    {
        var clientUuid = await GetPermissionsClientUuidAsync(cancellationToken);
        var permissions = await GetPermissionCatalogAsync(clientUuid, cancellationToken);

        return permissions
            .Where(p => p.Name is not null)
            .Select(p => new IdentityPermissionDto(p.Name!, p.Description))
            .OrderBy(p => p.Name, StringComparer.Ordinal)
            .ToList();
    }

    public async Task CreateRoleAsync(string name, string? description, IReadOnlyList<string> permissions, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"admin/realms/{_realm}/roles", new RoleRepresentation { Name = name, Description = description }, cancellationToken);
        EnsureSuccess(response, nameof(name));

        await SetRolePermissionsAsync(name, permissions, cancellationToken);
    }

    public async Task UpdateRoleAsync(string name, string? description, IReadOnlyList<string> permissions, CancellationToken cancellationToken)
    {
        // Keycloak's role update only touches name/description/attributes — composites are
        // managed separately below.
        var response = await _httpClient.PutAsJsonAsync(
            RolePath(name), new RoleRepresentation { Name = name, Description = description }, cancellationToken);
        response.EnsureSuccessStatusCode();

        await SetRolePermissionsAsync(name, permissions, cancellationToken);
    }

    public async Task DeleteRoleAsync(string name, CancellationToken cancellationToken)
    {
        var response = await _httpClient.DeleteAsync(RolePath(name), cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private string RolePath(string name) => $"admin/realms/{_realm}/roles/{Uri.EscapeDataString(name)}";

    private static bool IsAssignable(RoleRepresentation role) =>
        role.Name is not null && !role.Name.StartsWith(DefaultRolesPrefix, StringComparison.OrdinalIgnoreCase);

    private async Task SetRolePermissionsAsync(string roleName, IReadOnlyList<string> permissions, CancellationToken cancellationToken)
    {
        var clientUuid = await GetPermissionsClientUuidAsync(cancellationToken);
        var catalog = await GetPermissionCatalogAsync(clientUuid, cancellationToken);
        var target = catalog.Where(p => p.Name is not null && permissions.Contains(p.Name, StringComparer.Ordinal)).ToList();

        var current = await GetRolePermissionRepresentationsAsync(roleName, clientUuid, cancellationToken);

        await ReplaceMappingsAsync($"{RolePath(roleName)}/composites", current, target, cancellationToken);
    }

    /// <summary>
    /// Diffs <paramref name="current"/> against <paramref name="target"/> (by role ID) and POSTs
    /// the additions / DELETEs the removals at <paramref name="path"/> — the shape shared by
    /// Keycloak's user role-mapping and role-composite endpoints.
    /// </summary>
    private async Task ReplaceMappingsAsync(
        string path,
        IReadOnlyList<RoleRepresentation> current,
        IReadOnlyList<RoleRepresentation> target,
        CancellationToken cancellationToken)
    {
        var toAdd = target.Where(t => current.All(c => c.Id != t.Id)).ToList();
        var toRemove = current.Where(c => target.All(t => t.Id != c.Id)).ToList();

        if (toAdd.Count > 0)
        {
            var response = await _httpClient.PostAsJsonAsync(path, toAdd, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        if (toRemove.Count > 0)
        {
            // HttpClient.DeleteAsync can't carry a body, which these endpoints require.
            using var request = new HttpRequestMessage(HttpMethod.Delete, path) { Content = JsonContent.Create(toRemove) };
            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
    }

    private async Task<IdentityUserDto> ToUserDtoAsync(UserRepresentation user, CancellationToken cancellationToken)
    {
        var roles = await GetUserRoleRepresentationsAsync(user.Id!, cancellationToken);

        return new IdentityUserDto
        {
            Id = user.Id ?? string.Empty,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Enabled = user.Enabled,
            Roles = roles.Select(r => r.Name!).ToList(),
        };
    }

    private async Task<IdentityRoleDto> ToRoleDtoAsync(RoleRepresentation role, string clientUuid, CancellationToken cancellationToken)
    {
        var permissions = await GetRolePermissionRepresentationsAsync(role.Name!, clientUuid, cancellationToken);

        return new IdentityRoleDto
        {
            Id = role.Id ?? string.Empty,
            Name = role.Name!,
            Description = role.Description,
            Permissions = permissions.Where(p => p.Name is not null).Select(p => p.Name!).OrderBy(p => p, StringComparer.Ordinal).ToList(),
        };
    }

    /// <summary>The user's directly-assigned roles, minus Keycloak's built-in default role.</summary>
    private async Task<List<RoleRepresentation>> GetUserRoleRepresentationsAsync(string userId, CancellationToken cancellationToken)
    {
        var roles = await _httpClient.GetFromJsonAsync<List<RoleRepresentation>>(
            $"admin/realms/{_realm}/users/{userId}/role-mappings/realm", cancellationToken) ?? [];

        return roles.Where(IsAssignable).ToList();
    }

    private async Task<List<RoleRepresentation>> GetAssignableRealmRolesAsync(CancellationToken cancellationToken)
    {
        var roles = await _httpClient.GetFromJsonAsync<List<RoleRepresentation>>(
            $"admin/realms/{_realm}/roles", cancellationToken) ?? [];

        return roles.Where(IsAssignable).ToList();
    }

    private async Task<List<RoleRepresentation>> GetPermissionCatalogAsync(string clientUuid, CancellationToken cancellationToken)
    {
        return await _httpClient.GetFromJsonAsync<List<RoleRepresentation>>(
            $"admin/realms/{_realm}/clients/{clientUuid}/roles", cancellationToken) ?? [];
    }

    private async Task<List<RoleRepresentation>> GetRolePermissionRepresentationsAsync(
        string roleName, string clientUuid, CancellationToken cancellationToken)
    {
        return await _httpClient.GetFromJsonAsync<List<RoleRepresentation>>(
            $"{RolePath(roleName)}/composites/clients/{clientUuid}", cancellationToken) ?? [];
    }

    /// <summary>Client-scoped admin endpoints take the client's internal UUID, not its clientId.</summary>
    private async Task<string> GetPermissionsClientUuidAsync(CancellationToken cancellationToken)
    {
        var clients = await _httpClient.GetFromJsonAsync<List<ClientRepresentation>>(
            $"admin/realms/{_realm}/clients?clientId={Uri.EscapeDataString(_permissionsClientId)}", cancellationToken) ?? [];

        return clients.FirstOrDefault()?.Id
            ?? throw new InvalidOperationException($"Keycloak client '{_permissionsClientId}' was not found in realm '{_realm}'.");
    }

    /// <summary>
    /// Translates a 409 Conflict (duplicate username/role name) into the same
    /// <see cref="ValidationException"/> shape FluentValidation produces, so the API returns a
    /// normal 400 with field errors instead of a raw HTTP failure — a backstop for races the
    /// command validators' own uniqueness checks can't fully rule out.
    /// </summary>
    private static void EnsureSuccess(HttpResponseMessage response, string fieldName)
    {
        if (response.IsSuccessStatusCode) return;

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            throw new ValidationException(
                [new ValidationFailure(fieldName, "already exists.") { ErrorCode = "Unique" }]);
        }

        response.EnsureSuccessStatusCode();
    }
}
