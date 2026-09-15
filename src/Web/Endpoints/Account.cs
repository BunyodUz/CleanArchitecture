using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Web.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CleanArchitecture.Web.Endpoints;

public record CurrentUserDto(bool IsAuthenticated, string? UserName, IReadOnlyList<string> Roles, IReadOnlyList<string> Permissions);

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
    [EndpointDescription("Returns whether the caller is authenticated and, if so, their username, roles, and permissions.")]
    public static Ok<CurrentUserDto> GetCurrentUser(HttpContext httpContext, IUser user)
    {
        var isAuthenticated = httpContext.User.Identity?.IsAuthenticated ?? false;

        return TypedResults.Ok(new CurrentUserDto(
            isAuthenticated,
            isAuthenticated ? user.UserName : null,
            isAuthenticated ? user.Roles ?? [] : [],
            isAuthenticated ? user.Permissions ?? [] : []));
    }
}
