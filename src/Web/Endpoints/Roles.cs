using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Roles.Commands.CreateRole;
using CleanArchitecture.Application.Roles.Commands.DeleteRole;
using CleanArchitecture.Application.Roles.Commands.UpdateRole;
using CleanArchitecture.Application.Roles.Queries.GetPermissions;
using CleanArchitecture.Application.Roles.Queries.GetRoles;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CleanArchitecture.Web.Endpoints;

public class Roles : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetRoles);
        groupBuilder.MapGet(GetPermissions, "permissions");
        groupBuilder.MapPost(CreateRole);
        groupBuilder.MapPut(UpdateRole, "{name}");
        groupBuilder.MapDelete(DeleteRole, "{name}");
    }

    [EndpointSummary("Get all roles")]
    [EndpointDescription("Retrieves every assignable role along with the permissions it grants.")]
    public static async Task<Ok<IReadOnlyList<IdentityRoleDto>>> GetRoles(ISender sender)
    {
        var roles = await sender.Send(new GetRolesQuery());

        return TypedResults.Ok(roles);
    }

    [EndpointSummary("Get the permission catalog")]
    [EndpointDescription("Retrieves every permission that can be granted to a role.")]
    public static async Task<Ok<IReadOnlyList<IdentityPermissionDto>>> GetPermissions(ISender sender)
    {
        var permissions = await sender.Send(new GetPermissionsQuery());

        return TypedResults.Ok(permissions);
    }

    [EndpointSummary("Create a new role")]
    [EndpointDescription("Creates a role granting the given permissions.")]
    public static async Task<Created> CreateRole(ISender sender, CreateRoleCommand command)
    {
        await sender.Send(command);

        return TypedResults.Created($"/{nameof(Roles)}/{Uri.EscapeDataString(command.Name)}");
    }

    [EndpointSummary("Update a role")]
    [EndpointDescription("Updates the role's description and replaces its permissions. The name in the URL must match the name in the payload.")]
    public static async Task<Results<NoContent, BadRequest>> UpdateRole(ISender sender, string name, UpdateRoleCommand command)
    {
        if (name != command.Name) return TypedResults.BadRequest();

        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete a role")]
    [EndpointDescription("Deletes the role and removes it from every user who held it.")]
    public static async Task<NoContent> DeleteRole(ISender sender, string name)
    {
        await sender.Send(new DeleteRoleCommand(name));

        return TypedResults.NoContent();
    }
}
