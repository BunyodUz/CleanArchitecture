using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Users.Commands.CreateUser;
using CleanArchitecture.Application.Users.Commands.DeleteUser;
using CleanArchitecture.Application.Users.Commands.ResetPassword;
using CleanArchitecture.Application.Users.Commands.SetUserRoles;
using CleanArchitecture.Application.Users.Commands.UpdateUser;
using CleanArchitecture.Application.Users.Queries.GetRoles;
using CleanArchitecture.Application.Users.Queries.GetUser;
using CleanArchitecture.Application.Users.Queries.GetUsers;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CleanArchitecture.Web.Endpoints;

public class Users : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetUsers);
        groupBuilder.MapGet(GetRoles, "roles");
        groupBuilder.MapGet(GetUser, "{id}");
        groupBuilder.MapPost(CreateUser);
        groupBuilder.MapPut(UpdateUser, "{id}");
        groupBuilder.MapDelete(DeleteUser, "{id}");
        groupBuilder.MapPut(ResetPassword, "{id}/reset-password");
        groupBuilder.MapPut(SetUserRoles, "{id}/roles");
    }

    [EndpointSummary("Get all users")]
    [EndpointDescription("Retrieves users from Keycloak, optionally filtered by a search term matched against username, email, first and last name.")]
    public static async Task<Ok<IReadOnlyList<IdentityUserDto>>> GetUsers(ISender sender, string? search = null)
    {
        var users = await sender.Send(new GetUsersQuery(search));

        return TypedResults.Ok(users);
    }

    [EndpointSummary("Get the assignable realm roles")]
    [EndpointDescription("Retrieves the realm roles that can be assigned to a user.")]
    public static async Task<Ok<IReadOnlyList<IdentityRoleDto>>> GetRoles(ISender sender)
    {
        var roles = await sender.Send(new GetRolesQuery());

        return TypedResults.Ok(roles);
    }

    [EndpointSummary("Get a user")]
    [EndpointDescription("Retrieves the specified user, including their assigned realm roles.")]
    public static async Task<Ok<IdentityUserDto>> GetUser(ISender sender, string id)
    {
        var user = await sender.Send(new GetUserQuery(id));

        return TypedResults.Ok(user);
    }

    [EndpointSummary("Create a new user")]
    [EndpointDescription("Creates a new Keycloak user and returns their ID.")]
    public static async Task<Created<string>> CreateUser(ISender sender, CreateUserCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Users)}/{id}", id);
    }

    [EndpointSummary("Update a user")]
    [EndpointDescription("Updates the specified user's profile and enabled state. The ID in the URL must match the ID in the payload.")]
    public static async Task<Results<NoContent, BadRequest>> UpdateUser(ISender sender, string id, UpdateUserCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();

        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete a user")]
    [EndpointDescription("Deletes the user with the specified ID.")]
    public static async Task<NoContent> DeleteUser(ISender sender, string id)
    {
        await sender.Send(new DeleteUserCommand(id));

        return TypedResults.NoContent();
    }

    [EndpointSummary("Reset a user's password")]
    [EndpointDescription("Sets a new password for the specified user.")]
    public static async Task<Results<NoContent, BadRequest>> ResetPassword(ISender sender, string id, ResetPasswordCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();

        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [EndpointSummary("Set a user's realm roles")]
    [EndpointDescription("Replaces the specified user's realm role assignments.")]
    public static async Task<Results<NoContent, BadRequest>> SetUserRoles(ISender sender, string id, SetUserRolesCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();

        await sender.Send(command);

        return TypedResults.NoContent();
    }
}
