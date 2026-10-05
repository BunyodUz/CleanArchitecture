using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Infrastructure.Data;

public class AuditLog : IAuditLog
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<AuditLog> _logger;

    public AuditLog(IApplicationDbContext context, IUser user, TimeProvider timeProvider, ILogger<AuditLog> logger)
    {
        _context = context;
        _user = user;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task RecordAsync(
        string action,
        string targetType,
        string? targetId,
        string? targetName,
        string? details,
        CancellationToken cancellationToken)
    {
        _context.AuditEntries.Add(new AuditEntry
        {
            Timestamp = _timeProvider.GetUtcNow(),
            ActorId = _user.Id,
            ActorName = _user.UserName,
            Action = action,
            TargetType = targetType,
            TargetId = targetId,
            TargetName = targetName,
            Details = details,
        });

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // The change itself already succeeded in Keycloak, so failing the request now would
            // only invite a retry of something that's done. Log loudly instead.
            _logger.LogError(ex, "Failed to record audit entry {Action} for {TargetType} {TargetId} by {ActorId}",
                action, targetType, targetId, _user.Id);
        }
    }
}
