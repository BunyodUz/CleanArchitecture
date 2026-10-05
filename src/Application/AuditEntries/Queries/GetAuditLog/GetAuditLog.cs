using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Security;
using CleanArchitecture.Domain.Constants;

namespace CleanArchitecture.Application.AuditEntries.Queries.GetAuditLog;

/// <summary>A page of the audit log, newest first.</summary>
[Authorize(Permissions = Permissions.Audit.Read)]
public record GetAuditLogQuery : IRequest<AuditLogPageDto>
{
    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}

public class GetAuditLogQueryHandler : IRequestHandler<GetAuditLogQuery, AuditLogPageDto>
{
    private readonly IApplicationDbContext _context;

    public GetAuditLogQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AuditLogPageDto> Handle(GetAuditLogQuery request, CancellationToken cancellationToken)
    {
        var totalCount = await _context.AuditEntries.CountAsync(cancellationToken);

        var items = await _context.AuditEntries
            .AsNoTracking()
            .OrderByDescending(e => e.Timestamp)
            .ThenByDescending(e => e.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => new AuditEntryDto
            {
                Id = e.Id,
                Timestamp = e.Timestamp,
                ActorId = e.ActorId,
                ActorName = e.ActorName,
                Action = e.Action,
                TargetType = e.TargetType,
                TargetId = e.TargetId,
                TargetName = e.TargetName,
                Details = e.Details,
            })
            .ToListAsync(cancellationToken);

        return new AuditLogPageDto
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
        };
    }
}
