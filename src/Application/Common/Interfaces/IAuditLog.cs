namespace CleanArchitecture.Application.Common.Interfaces;

/// <summary>
/// Records administrative changes against the current user (<see cref="IUser"/>). Call it after
/// the change has succeeded.
/// </summary>
public interface IAuditLog
{
    /// <param name="action">One of the <see cref="Domain.Constants.AuditActions"/> values.</param>
    /// <param name="targetType">One of the <see cref="Domain.Constants.AuditTargets"/> values.</param>
    /// <param name="targetId">The changed object's id (the Keycloak user id, or the role name).</param>
    /// <param name="targetName">A readable name for the changed object (username or role name).</param>
    /// <param name="details">Optional summary of what changed.</param>
    Task RecordAsync(
        string action,
        string targetType,
        string? targetId,
        string? targetName,
        string? details,
        CancellationToken cancellationToken);
}
