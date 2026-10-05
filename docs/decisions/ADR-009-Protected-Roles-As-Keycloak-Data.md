# ADR-009: Protected Roles as Keycloak Data, with a Last-Administrator Guard

## Status

Accepted (amends the lockout guard in ADR-007)

## Date

2026-10-05

## Context

ADR-007 protected the `Administrator` role from being edited or deleted, so the last administrator couldn't remove their own ability to fix things. It did this by name: `Domain.Constants.Roles.Administrator` was compared in the role validators, and the frontend compared the same string to show a "Built-in" badge and to count administrators.

This is a solution template. Whoever adopts it decides their own roles, and the realm export's roles are only a starting point. With the name hard-coded, renaming `Administrator` in the realm silently dropped the lockout protection.

The protection also had gaps. It stopped the role itself from being changed, but not:

- removing the role from the last user who held it,
- disabling that user, or
- deleting that user.

Each of these locked everyone out of user and role management.

## Decision

- **Which roles are protected is data in Keycloak.** A realm role is protected when it has the attribute `protected = true`. The realm export marks the starting role, `Administrator`, this way. `IdentityRoleDto.IsProtected` carries the flag, and the app no longer knows any role by name (`Domain.Constants.Roles` is removed).
- **Protected roles can't be edited or deleted** from the app (`UpdateRoleCommandValidator`, `DeleteRoleCommandValidator`). Updating any role sends its existing attributes back, so an edit can never drop the flag.
- **The last administrator can't be removed.** An *administrator* here means an enabled user directly holding at least one protected role. `SetUserRolesCommand`, `UpdateUserCommand` (when disabling) and `DeleteUserCommand` refuse a change that would leave no administrator. They return a validation error that names the protected roles. The rule lives in `Application/Common/Security/ProtectedRoleGuard`.
- The frontend shows a "Protected" badge from the flag and counts users holding a protected role.
- `Member` and the two dev users in the realm export are labelled as samples, alongside the todo feature.

## Rationale

### A Keycloak role attribute, not configuration

An `appsettings` list of protected role names would also remove the constant. But it would put role data in two places that can drift apart: renaming a role in Keycloak would still silently drop the protection. An attribute travels with the role, and the realm export already defines the roles.

The app can't set or clear the attribute, so an administrator can't unprotect a role from the UI. Changing which roles are protected needs Keycloak admin-console access, which is the right bar for it.

### Enabled, direct holders only

A disabled user can't sign in, so they don't count. The guard checks direct role assignments, which is how the app assigns roles. Holding a protected role only through a group or another composite role isn't counted. That errs on the side of refusing a change rather than allowing a lockout.

## Consequences

**Easier:**
- Adopters can rename, add or remove roles, including the protected starting role, by editing only the realm export (or the Keycloak console). No code changes are needed.
- More than one role can be protected.
- None of the usual admin actions in the app can lock everyone out of administration.

**Harder:**
- Removing roles from, disabling or deleting a user who holds a protected role costs extra Keycloak calls: the role list, then each protected role's members.
- The guard isn't transactional. Two administrators demoting each other at the same moment could both pass the check. That needs two admins acting at once, and recovery is through the Keycloak admin console.
- Existing dev environments must re-import the realm to get the attribute. Until then no role is protected, and the guard allows every change.
