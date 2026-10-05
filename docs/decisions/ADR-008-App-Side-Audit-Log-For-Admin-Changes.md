# ADR-008: App-Side Audit Log for Administrative Changes

## Status

Accepted

## Date

2026-10-05

## Context

ADR-006 and ADR-007 put user and role management in the app: administrators create and edit users, assign roles, reset passwords and reshape roles from the admin area. Those changes are security-relevant, and "who gave this person Administrator, and when?" needs an answer.

Keycloak can record admin events itself, but every change the app makes goes through the `cleanarchitecture-admin-api` service account (ADR-006). Keycloak's log would therefore attribute every change to that one account, not to the administrator who clicked the button. The real actor is only known to the app, as the signed-in user on the request (`IUser`).

## Decision

The app records its own audit log of administrative changes:

- **`AuditEntry`** (Domain entity, `AuditEntries` table, created by DbUp script `0002_AuditLog.sql`) stores the timestamp, the actor's Keycloak id and a snapshot of their username, the action (`AuditActions`, e.g. `user.roles_changed`), the target type, id and name, and a readable `Details` summary such as `roles: +Auditor, −Member` or `status: enabled → disabled`.
- **`IAuditLog.RecordAsync`** (Application interface, implemented in Infrastructure) is called explicitly by each user and role command handler *after* the Keycloak call succeeds. Handlers that change existing objects read the current state first, so the entry can record the difference. A change that changes nothing (for example, saving the user dialog with the same roles) is not recorded.
- Passwords are never recorded. A password reset is logged with only whether the new password is temporary.
- Reading the log needs a new permission, **`audit.read`**, which is part of `Administrator`. It is served by `GET /api/AuditLog` (paged, newest first) and shown on the **Audit log** admin page and the overview's **Recent changes** card.
- If writing the entry fails after the Keycloak change has succeeded, the failure is logged as an error and the request still succeeds.

## Rationale

### Explicit calls in handlers, not a MediatR pipeline behaviour

A generic behaviour could record every command automatically, but it only sees the request. It can't name a deleted user (the request only carries an id), and it can't describe *what changed* without each command teaching it how. Calling `IAuditLog` from the handler keeps that knowledge next to the code that makes the change. The cost is that a new admin command must remember to record itself; the unit tests in `UserAuditTests` cover the existing ones.

### Not failing the request when the audit write fails

The audit write happens after Keycloak has already applied the change, so the two can't share a transaction. Returning an error would tell the administrator the change failed when it didn't, which invites a retry (and, for creation, a duplicate-username error). Logging the failure loudly is the more honest outcome. A deployment that needs a guaranteed trail should also enable Keycloak's admin events as a backstop.

## Consequences

**Easier:**
- Every user and role change has a named actor, a time and a readable summary, visible in the app without access to Keycloak.
- Actor and target names are snapshots, so entries stay meaningful after either is deleted.

**Harder:**
- Changes made directly in the Keycloak admin console bypass the app and aren't in this log.
- Update, delete, role and password handlers make one extra Keycloak read to capture the "before" state.
- The log is append-only and grows without bound; there is no retention policy yet.
- Existing dev environments must re-import the realm to get the `audit.read` permission (see ADR-007's consequences).
