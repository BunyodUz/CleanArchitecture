# ADR-006: User Management via Keycloak's Admin REST API

## Status

Accepted

## Date

2026-09-16

## Context

ADR-005 moved authentication and the user/role store to Keycloak, and explicitly accepted the consequence that adding or managing a user became a Keycloak-side operation with "no code path for user/role management to build" in this app. In practice, that means every user has to be created and every role assigned through Keycloak's own Admin Console (or the static `deploy/keycloak/realm-export.json` seed) — there's no in-app way to do it, which is a gap for a template meant to demonstrate a complete, usable admin experience.

There is still no local user store — that decision stands. The question this ADR answers is how to manage the users that live in Keycloak from inside this app.

## Decision

Add a `/api/Users` endpoint group backed entirely by Keycloak's [Admin REST API](https://www.keycloak.org/docs-api/latest/rest-api/index.html#_users), following the same CQRS shape as every other feature:

- `Application/Common/Interfaces/IIdentityAdminService` abstracts the operations this app needs: list/get/create/update/delete users, reset a password, list assignable realm roles, and replace a user's realm role assignments.
- `Infrastructure/Identity/KeycloakAdminService` implements it over `HttpClient`, calling `{baseUrl}/admin/realms/{realm}/...` directly — there is no Keycloak admin SDK dependency.
- A dedicated confidential client, `cleanarchitecture-admin-api`, is added to the realm (`deploy/keycloak/realm-export.json`) with `serviceAccountsEnabled: true` and its service account granted `realm-management` client roles (`view-realm`, `view-users`, `query-users`, `query-groups`, `manage-users`) — scoped to exactly what user administration needs, nothing realm-wide. `KeycloakAdminAuthHandler` fetches and caches a client-credentials token for this client, separately from the `cleanarchitecture-web` client used for the BFF login flow.
- Every `Users` command/query carries `[Authorize(Roles = Roles.Administrator)]` — user management is a whole-app admin action, not a per-resource permission, so it uses the `Roles` tier of the three-tier authorization model from ADR-005 rather than adding new `Permissions` constants.
- The frontend adds a `views/users` admin screen (list, create, edit, delete, reset password, role assignment) behind `AuthGuard requireRole="Administrator"`, and a "Users" nav link visible only to administrators.

## Rationale

### A dedicated admin-api client, not reusing `cleanarchitecture-web`

`cleanarchitecture-web`'s client secret is what the browser-facing OIDC flow trusts; giving it `realm-management` roles as well would mean a leak of that one secret compromises both authentication and full user administration. A second client scoped only to the Admin API's service account keeps the blast radius of each secret to what it actually does.

### Direct HTTP calls, not a Keycloak admin client SDK

Keycloak's Admin REST API is small enough (a handful of endpoints, all plain JSON) that a typed SDK dependency buys little beyond what `HttpClient` + a few DTOs already provide, and keeps this template's dependency footprint the same as every other outbound call it makes.

### Roles, not Permissions, for the gate

Per ADR-005, `Permissions` answers "can this user do X" for a specific resource/feature; `Roles` answers "what is this user" for whole-app gates. User management is squarely the second question — there's no finer-grained shape to it (no "can edit some users but not others"), so it uses `[Authorize(Roles = Roles.Administrator)]` rather than inventing `users.read`/`users.write` permissions that would only ever be held by administrators anyway.

## Consequences

**Easier:**
- Administrators can create users, reset passwords, and assign roles without leaving the app or touching Keycloak's Admin Console.
- The realm's role list stays the single source of truth for what's assignable — `GetRolesQuery` reads it live from Keycloak rather than duplicating it in this app.

**Harder:**
- User list/detail views now depend on Keycloak's Admin API being reachable; an outage there degrades user management (not authentication, which still works independently) — mitigated with a standard resilience handler (retry/circuit breaker), same as any other outbound call.
- Listing users fetches each user's role mappings with a separate request (Keycloak has no bulk "users with roles" endpoint), which is fine for the modest user counts this template targets but wouldn't scale to a large realm without adding pagination and caching.
- A second client secret (`cleanarchitecture-admin-api`) now needs to be provisioned and rotated in every environment, alongside the existing `cleanarchitecture-web` one.
