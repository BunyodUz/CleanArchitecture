# ADR-005: Keycloak Authentication and Permission-Based Authorization

## Status

Accepted

## Date

2026-09-15

## Context

The template previously used ASP.NET Core Identity: a local `AspNetUsers`/`AspNetRoles` schema in the app's own database, `MapIdentityApi<ApplicationUser>()` for registration/login/password-reset endpoints, and role-based checks (`[Authorize(Roles = "Administrator")]`) as the only fine-grained authorization mechanism — in practice barely used, since almost every command/query had no authorization beyond "must be logged in".

This fork now uses [Keycloak](https://www.keycloak.org/) as the identity provider. Authentication and authorization are separate concerns and this ADR covers both, since replacing the identity provider also removed the local user/role store that the old role checks depended on.

## Decision

### Authentication: Keycloak via the BFF pattern

The Web app is a confidential OIDC client. It drives the Authorization Code + PKCE flow with Keycloak server-side (`AddAuthentication().AddCookie().AddOpenIdConnect()`) and hands the browser an `httpOnly` session cookie — the Angular SPA never sees an access or id token. `/account/login`, `/account/logout`, and `/account/user` (`src/Web/Endpoints/Account.cs`) are the only endpoints the SPA talks to for auth; login and logout are full-page redirects, not API calls. Keycloak's own hosted pages are the login/registration UI — the SPA no longer has its own login/register forms.

For the `UseApiOnly` variant (no SPA to hold a session for), the Web API instead validates bearer tokens directly (`AddJwtBearer`), since there's no browser session to broker.

There is no more local user store. `AspNetUsers`/`AspNetRoles` and everything under `src/Infrastructure/Identity` are gone; `CreatedBy`/`LastModifiedBy` on audited entities store the Keycloak user id (the token's `sub` claim) as a plain string.

### Authorization: three tiers, permissions as the default

A `KeycloakClaimsTransformation` (`src/Web/Services`) maps Keycloak's token claims onto the `ClaimsPrincipal` on every request:

- `realm_access.roles` → `ClaimTypes.Role` claims — coarse, whole-app roles (e.g. `Administrator`).
- `resource_access.{clientId}.roles` → `permission` claims — fine-grained, e.g. `todolists.write` (see `Domain.Constants.Permissions`).

`[Authorize]` (`src/Application/Common/Security/AuthorizeAttribute.cs`) on a MediatR request now has three independent properties, checked by `AuthorizationBehaviour`:

- **`Roles`** — coarse gate, checked against `IUser.Roles`. Use for whole-app admin-only actions.
- **`Permissions`** — the default choice for "can this user do X". Checked directly against `IUser.Permissions`, no round-trip.
- **`Policy`** — an escape hatch evaluated via ASP.NET Core's real `IAuthorizationService`, for resource/ownership checks a static claim can't express (e.g. "is this the owner of this specific record"). Nothing in this codebase needs it yet, since `TodoList`/`TodoItem` have no ownership dimension, but the mechanism is there via `IUser.Principal`.

`IIdentityService` is gone. It previously existed only to let the MediatR pipeline (which has no `HttpContext`) re-derive a `ClaimsPrincipal` from a user id via `UserManager`/`IUserClaimsPrincipalFactory`. `IUser` (backed by `IHttpContextAccessor`) already exposes the live authenticated principal's claims directly, so that indirection had nothing left to do — `GetUserNameAsync`/`AuthorizeAsync` collapsed into `IUser.UserName`/`IUser.Permissions`/`IUser.Principal`.

## Rationale

### BFF over a public client

A public client (SPA holds tokens directly, PKCE, no secret) was the other option. BFF was chosen because the access token never reaches browser-accessible storage or JS-readable memory the SPA's own code touches — the cookie is the only thing the browser holds, and it's opaque to the SPA. This is the pattern [OAuth 2.0 for Browser-Based Apps](https://datatracker.ietf.org/doc/html/draft-ietf-oauth-browser-based-apps) recommends for exactly this shape of app (a SPA served by the same backend it calls).

### Permissions over roles as the default

Roles answer "what is this user" (Administrator, Member); permissions answer "can this user do X" (`todolists.write`). Most authorization decisions in a CRUD-shaped app are the second question, and modeling them as roles either explodes the role count (`TodoListEditor`, `TodoListViewer`, ...) or leaves checks too coarse to be useful (falling back to "any authenticated user can do anything", which is what this template had before). Permissions-as-claims keep the check a direct list-contains against the token — no database round-trip, no re-derivation.

### Policy kept as an escape hatch, not the default

ASP.NET Core's policy/requirement/handler model is the right tool for resource-based checks (does this user own *this* record), but it's overkill as the default mechanism for simple claim checks — it exists in this codebase for when permissions alone aren't enough, not to replace them.

## Consequences

**Easier:**
- No local user/password/token storage or lifecycle to secure, migrate, or reason about — Keycloak owns all of it.
- `AuthorizationBehaviour` checks are direct claim lookups; no service call, no database round-trip.
- Adding a new permission is a new constant in `Permissions` plus a new client role in the Keycloak realm — no code path for user/role management to build.

**Harder:**
- A local dev environment now needs Keycloak running (via Aspire, see `src/AppHost/Program.cs`) in addition to Postgres.
- Entity/config changes that need a new permission require a matching change in the realm (a new client role) — nothing generates that from code, the same tradeoff already accepted for the DB schema in ADR-004.
- Self-registration and password reset are Keycloak's hosted pages now, not this app's UI — customizing that experience means customizing a Keycloak theme, not an Angular component.
