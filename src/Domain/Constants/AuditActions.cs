namespace CleanArchitecture.Domain.Constants;

/// <summary>The <see cref="Entities.AuditEntry.Action"/> values the app records.</summary>
public abstract class AuditActions
{
    public const string UserCreated = "user.created";
    public const string UserUpdated = "user.updated";
    public const string UserDeleted = "user.deleted";
    public const string UserPasswordReset = "user.password_reset";
    public const string UserRolesChanged = "user.roles_changed";

    public const string RoleCreated = "role.created";
    public const string RoleUpdated = "role.updated";
    public const string RoleDeleted = "role.deleted";
}

/// <summary>The <see cref="Entities.AuditEntry.TargetType"/> values.</summary>
public abstract class AuditTargets
{
    public const string User = "User";
    public const string Role = "Role";
}
