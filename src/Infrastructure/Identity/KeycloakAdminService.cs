using System.Net;
using System.Net.Http.Json;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using FluentValidation.Results;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Infrastructure.Identity;

/// <summary>
/// Implements <see cref="IIdentityAdminService"/> against Keycloak's Admin REST API. There is no
/// local Users table — every operation here is a direct HTTP call to Keycloak (see ADR-006).
/// </summary>
public class KeycloakAdminService : IIdentityAdminService
{
    // Keycloak gives every realm a synthetic composite role assigning its own default client
    // scopes — it's not something an admin ever assigns/unassigns by hand, so it's filtered out
    // of the assignable-roles list.
    private const string DefaultRolesPrefix = "default-roles-";

    private readonly HttpClient _httpClient;
    private readonly string _realm;

    public KeycloakAdminService(HttpClient httpClient, IOptions<KeycloakAdminOptions> options)
    {
        _httpClient = httpClient;
        _realm = options.Value.Realm;
    }

    public async Task<IReadOnlyList<IdentityUserDto>> GetUsersAsync(string? search, CancellationToken cancellationToken)
    {
        var query = string.IsNullOrWhiteSpace(search) ? string.Empty : $"?search={Uri.EscapeDataString(search)}";

        var users = await _httpClient.GetFromJsonAsync<List<UserRepresentation>>(
            $"admin/realms/{_realm}/users{query}", cancellationToken) ?? [];

        var result = new List<IdentityUserDto>(users.Count);
        foreach (var user in users)
        {
            result.Add(await ToDtoAsync(user, cancellationToken));
        }

        return result;
    }

    public async Task<IdentityUserDto?> GetUserAsync(string id, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"admin/realms/{_realm}/users/{id}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;

        response.EnsureSuccessStatusCode();

        var user = await response.Content.ReadFromJsonAsync<UserRepresentation>(cancellationToken);

        return user is null ? null : await ToDtoAsync(user, cancellationToken);
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
        await EnsureSuccessAsync(response, nameof(username));

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
        await EnsureSuccessAsync(response, nameof(username));
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

    public async Task<IReadOnlyList<IdentityRoleDto>> GetRolesAsync(CancellationToken cancellationToken)
    {
        var roles = await GetAllRealmRolesAsync(cancellationToken);

        return roles
            .Where(r => r.Name is not null && !r.Name.StartsWith(DefaultRolesPrefix, StringComparison.OrdinalIgnoreCase))
            .Select(r => new IdentityRoleDto(r.Id ?? string.Empty, r.Name ?? string.Empty))
            .OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task SetUserRolesAsync(string id, IReadOnlyList<string> roleNames, CancellationToken cancellationToken)
    {
        var allRoles = await GetAllRealmRolesAsync(cancellationToken);
        var targetRoles = allRoles.Where(r => r.Name is not null && roleNames.Contains(r.Name, StringComparer.OrdinalIgnoreCase)).ToList();

        var currentRoles = await GetUserRoleRepresentationsAsync(id, cancellationToken);

        var toAdd = targetRoles.Where(t => currentRoles.All(c => c.Id != t.Id)).ToList();
        var toRemove = currentRoles.Where(c => targetRoles.All(t => t.Id != c.Id)).ToList();

        if (toAdd.Count > 0)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"admin/realms/{_realm}/users/{id}/role-mappings/realm", toAdd, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        if (toRemove.Count > 0)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"admin/realms/{_realm}/users/{id}/role-mappings/realm")
            {
                Content = JsonContent.Create(toRemove),
            };
            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
    }

    private async Task<IdentityUserDto> ToDtoAsync(UserRepresentation user, CancellationToken cancellationToken)
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
            Roles = roles.Where(r => r.Name is not null).Select(r => r.Name!).ToList(),
        };
    }

    private async Task<List<RoleRepresentation>> GetUserRoleRepresentationsAsync(string userId, CancellationToken cancellationToken)
    {
        return await _httpClient.GetFromJsonAsync<List<RoleRepresentation>>(
            $"admin/realms/{_realm}/users/{userId}/role-mappings/realm", cancellationToken) ?? [];
    }

    private async Task<List<RoleRepresentation>> GetAllRealmRolesAsync(CancellationToken cancellationToken)
    {
        return await _httpClient.GetFromJsonAsync<List<RoleRepresentation>>(
            $"admin/realms/{_realm}/roles", cancellationToken) ?? [];
    }

    /// <summary>
    /// Translates a 409 Conflict (duplicate username/email) into the same
    /// <see cref="ValidationException"/> shape FluentValidation produces, so the API returns a
    /// normal 400 with field errors instead of a raw HTTP failure — a backstop for races the
    /// command validators' own uniqueness checks can't fully rule out.
    /// </summary>
    private static Task EnsureSuccessAsync(HttpResponseMessage response, string fieldName)
    {
        if (response.IsSuccessStatusCode) return Task.CompletedTask;

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            throw new ValidationException(
                [new ValidationFailure(fieldName, "already exists.") { ErrorCode = "Unique" }]);
        }

        response.EnsureSuccessStatusCode();
        return Task.CompletedTask;
    }
}
