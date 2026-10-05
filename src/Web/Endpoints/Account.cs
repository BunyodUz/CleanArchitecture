using System.Security.Claims;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Web.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CleanArchitecture.Web.Endpoints;

public record CurrentUserDto(
    bool IsAuthenticated,
    string? UserName,
    string? Email,
    string? FullName,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions,
    string? ManageAccountUrl);

public class Account : IEndpointGroup
{
    public static string? RoutePrefix => "/account";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(Login, "login");
        groupBuilder.MapGet(Logout, "logout");
        groupBuilder.MapGet(GetCurrentUser, "user");
    }

    [EndpointSummary("Log in")]
    [EndpointDescription("Redirects to Keycloak's hosted login page. On success, redirects back to returnUrl.")]
    public static IResult Login(string? returnUrl)
    {
        return Results.Challenge(
            new AuthenticationProperties { RedirectUri = returnUrl ?? "/" },
            [OpenIdConnectDefaults.AuthenticationScheme]);
    }

    [EndpointSummary("Log out")]
    [EndpointDescription("Clears the local session and ends the Keycloak session, then redirects to returnUrl.")]
    public static IResult Logout(string? returnUrl)
    {
        return Results.SignOut(
            new AuthenticationProperties { RedirectUri = returnUrl ?? "/" },
            [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]);
    }

    [EndpointSummary("Get current user")]
    [EndpointDescription("Returns whether the caller is authenticated and, if so, their profile, roles, and permissions.")]
    public static Ok<CurrentUserDto> GetCurrentUser(HttpContext httpContext, IUser user, IConfiguration configuration)
    {
        var principal = httpContext.User;
        if (principal.Identity?.IsAuthenticated != true)
        {
            return TypedResults.Ok(new CurrentUserDto(false, null, null, null, [], [], null));
        }

        // The OIDC handler maps inbound claims by default ("email" → ClaimTypes.Email, etc.), so
        // check both the mapped and the raw JWT claim names.
        string? Claim(string mapped, string raw) => principal.FindFirstValue(mapped) ?? principal.FindFirstValue(raw);

        var fullName = string.Join(' ', new[] { Claim(ClaimTypes.GivenName, "given_name"), Claim(ClaimTypes.Surname, "family_name") }
            .Where(part => !string.IsNullOrWhiteSpace(part)));

        // Keycloak's account console (password, two-factor, sessions) lives under the realm's issuer URL.
        var authority = configuration["Authentication:Keycloak:Authority"]?.TrimEnd('/');

        return TypedResults.Ok(new CurrentUserDto(
            true,
            user.UserName,
            Claim(ClaimTypes.Email, "email"),
            fullName.Length > 0 ? fullName : null,
            user.Roles ?? [],
            user.Permissions ?? [],
            authority is null ? null : $"{authority}/account"));
    }
}
