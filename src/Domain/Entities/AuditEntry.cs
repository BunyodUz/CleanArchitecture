namespace CleanArchitecture.Domain.Entities;

/// <summary>
/// One administrative change (a user or role created, updated, deleted…), recorded by the app.
/// Keycloak's own admin events can't serve as the audit trail: every change goes through the
/// app's service account, so Keycloak would attribute all of them to that account rather than
/// to the administrator who made them. The actor's name is a snapshot, so entries stay readable
/// after the actor or the target is deleted.
/// </summary>
public class AuditEntry : BaseEntity
{
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>The Keycloak user id ("sub" claim) of the administrator who made the change.</summary>
    public string? ActorId { get; set; }

    public string? ActorName { get; set; }

    /// <summary>One of the <see cref="Constants.AuditActions"/> values, e.g. "user.created".</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>"User" or "Role".</summary>
    public string TargetType { get; set; } = string.Empty;

    public string? TargetId { get; set; }

    public string? TargetName { get; set; }

    /// <summary>Human-readable summary of what changed, e.g. "roles: +Auditor, −Member".</summary>
    public string? Details { get; set; }
}
