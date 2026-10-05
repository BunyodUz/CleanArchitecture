namespace CleanArchitecture.Application.AuditEntries.Queries.GetAuditLog;

public class AuditEntryDto
{
    public int Id { get; init; }

    public DateTimeOffset Timestamp { get; init; }

    public string? ActorId { get; init; }

    public string? ActorName { get; init; }

    public string Action { get; init; } = string.Empty;

    public string TargetType { get; init; } = string.Empty;

    public string? TargetId { get; init; }

    public string? TargetName { get; init; }

    public string? Details { get; init; }
}

public class AuditLogPageDto
{
    public IReadOnlyList<AuditEntryDto> Items { get; init; } = [];

    public int PageNumber { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }
}
