using CleanArchitecture.Application.AuditEntries.Queries.GetAuditLog;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CleanArchitecture.Web.Endpoints;

public class AuditLog : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetAuditLog);
    }

    [EndpointSummary("Get the audit log")]
    [EndpointDescription("Retrieves a page of administrative changes to users and roles, newest first. Requires audit.read.")]
    public static async Task<Ok<AuditLogPageDto>> GetAuditLog(ISender sender, int pageNumber = 1, int pageSize = 20)
    {
        var page = await sender.Send(new GetAuditLogQuery { PageNumber = pageNumber, PageSize = pageSize });

        return TypedResults.Ok(page);
    }
}
