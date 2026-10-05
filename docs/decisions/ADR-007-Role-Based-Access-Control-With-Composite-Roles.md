# ADR-007: Role-Based Access Control with Keycloak Composite Roles

## Status

Accepted. The lockout guard is amended by [ADR-009](ADR-009-Protected-Roles-As-Keycloak-Data.md): protected roles are now marked in Keycloak instead of by name.

## Date

2026-10-05

## Context

ADR-005 established permissions (client roles of `cleanarchitecture-web`, e.g. `todolists.write`) as the default authorization check, but left *how a user comes to hold a permission* unstructured: the realm export assigned permissions to each user directly, alongside a separate `Administrator` realm role. That has the usual problems of per-user grants — every new user needs the same list of permissions copied onto them, there's no single place to change what "a member" can do, and nothing ties `Administrator` to the permissions an administrator actually needs. ADR-006 then gated user management on `[Authorize(Roles = Administrator)]`, a second, parallel way of deciding access.

## Decision

Adopt classic RBAC — **users → roles → permissions** — modelled natively in Keycloak:

- **Permissions** stay client roles of `cleanarchitecture-web` (`Domain.Constants.Permissions`), now including `users.read/write` and `roles.read/write`. Every authorization check in the app is a permission check.
- **Roles** are realm roles made *composite* over permissions: `Administrator` holds every permission, `Member` holds the todo permissions. Keycloak expands composites when it issues tokens, so `KeycloakClaimsTransformation` and `AuthorizationBehaviour` are unchanged — a user's effective permissions simply arrive as `permission` claims.
- **Users are assigned roles only**, never permissions directly (the realm export no longer grants client roles to users).
- **Roles are managed in-app** (`/api/Roles`, the Roles page): list roles with their permissions, read the permission catalog, create a role, change its description/permissions, delete it. Implemented on Keycloak's role and role-composite Admin API endpoints via `IIdentityAdminService` (ADR-006).
- User management is re-gated on permissions (`users.read` / `users.write`) instead of the `Administrator` role.
- **Privilege-escalation guard**: assigning roles is how permissions are granted, so it requires `roles.write` *in addition to* `users.write` — on `SetUserRolesCommand` via `[Authorize]`, and in `CreateUserCommandHandler` when roles are supplied at creation. Without this, `users.write` alone would let a holder promote anyone, themselves included, to `Administrator`.
- **Lockout guard**: `Administrator` is a built-in role that can't be modified or deleted, so the last administrator can't remove their own ability to fix things.
- The frontend hides navigation and actions by permission (`AuthGuard requirePermission`, permission-aware nav/views). That's convenience only — the API enforces every rule.

## Rationale

### Composite roles in Keycloak, not a role→permission table in this app's database

The alternative was to keep only role *names* in Keycloak and store the role→permission mapping in the app's own database, resolving permissions per request. That would keep the permission catalog next to the code and avoid giving the admin-api service account realm-level rights. It was rejected because it splits the source of truth across two systems, adds a database lookup (or a cache to invalidate) to every authorization check, and means tokens issued to *other* clients of the realm (e.g. an API-only caller using bearer tokens) wouldn't carry permissions at all. With composites, a token from Keycloak is self-describing, which is what ADR-005's "permissions as claims" choice assumed.

### Permissions, not roles, at the check site

Checking `[Authorize(Roles = "Administrator")]` couples code to a specific bundling decision; renaming or splitting a role means editing code. Checking permissions means roles can be reshaped (a new "UserManager" role holding `users.*` only, say) purely as data, with no deploy.

## Consequences

**Easier:**
- New users get a coherent permission set by being given one role; changing what a role can do updates every holder at once.
- New roles can be composed from existing permissions at runtime from the Roles page.

**Harder:**
- `roles.write` is effectively full administrative power: a holder can add any permission to a role they hold. Grant it as deliberately as `Administrator` itself.
- Role management requires the `cleanarchitecture-admin-api` service account to hold `manage-realm` and `manage-clients` (Keycloak requires `manage-clients` to add a client role to a composite). That makes its secret effectively realm-admin; store and rotate it accordingly. User management alone only needed `manage-users`.
- Role/permission changes take effect at the user's next login: the BFF session cookie holds the claims from sign-in, and there's no push invalidation.
- Adding a *new permission* is still a code change (`Permissions` constant + its check) plus a realm change (the client role, and adding it to `Administrator`) — the catalog can't be extended from the UI, by design.
- Because the BFF builds its principal from the ID token, the web client needs protocol mappers putting `realm_access`/`resource_access` into the ID token (Keycloak's default `roles` scope only adds them to the access token). These are now in the realm export.
- Existing dev environments must re-import the realm (Keycloak only imports a realm that doesn't exist yet): remove the Keycloak data volume and restart the AppHost.
