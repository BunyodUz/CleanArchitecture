# ADR-005: Docker Compose Instead of Aspire

## Status

Accepted — supersedes [ADR-002](ADR-002-Aspire-For-Orchestration-And-Testing.md)

## Date

2026-09-15

## Context

[ADR-002](ADR-002-Aspire-For-Orchestration-And-Testing.md) adopted Aspire for local orchestration and testing: an `AppHost` project ran Postgres, the Web API, and the frontend dev server together; a `TestAppHost` project provisioned an ephemeral Postgres for `Application.FunctionalTests`; and `Web.AcceptanceTests` used Aspire's testing APIs to spin up the full stack for Playwright. `ServiceDefaults` (OpenTelemetry, health checks, HTTP resilience, service discovery) was wired in via the same package family.

This fork has since moved to a single target database (PostgreSQL, see [ADR-004](ADR-004-SQL-Script-Migrations-With-DbUp.md)) and does not need Aspire's multi-provider orchestration, its dashboard, or its dependency on a Microsoft-specific hosting model. Plain Docker Compose is a smaller, more transparent, and more widely understood way to run infrastructure dependencies locally.

## Decision

Aspire is removed. Infrastructure dependencies (currently just PostgreSQL; Redis, MinIO, etc. can be added the same way) run via a root `docker-compose.yml`. The Web API and its frontend are run directly (`dotnet run`, `npm start`) rather than orchestrated by an AppHost project. `ServiceDefaults` is kept as a plain (non-Aspire) project — it still wires up OpenTelemetry, `/health`/`/alive` endpoints, and HTTP resilience/service discovery, just without the Aspire package family behind it.

Tests no longer provision their own database: `Application.FunctionalTests` and `Web.AcceptanceTests` both assume `docker compose up -d` has already started Postgres, and connect to it via a fixed connection string (overridable through the `ConnectionStrings__CleanArchitectureDb` environment variable). `Web.AcceptanceTests` boots the Web app itself via a `WebApplicationFactory<Program>` configured to listen on a real Kestrel socket, giving Playwright a real URL without any orchestration project.

## Rationale

### Aspire's value here was mostly about multi-provider orchestration

Aspire's biggest wins — spinning up whichever database provider a solution variant chose, wiring services together by name, giving each variant a dashboard — matter most when there's more than one provider and more than one service to coordinate. With PostgreSQL as the only provider and no other backing services yet, most of that machinery had nothing left to coordinate.

### Docker Compose is what ADR-002 compared it against — the tradeoff just changed

ADR-002 rejected Docker Compose for lacking observability, service discovery, and programmatic test control, and for living outside the .NET solution. Those are real costs, but with a single, stable set of infrastructure dependencies they matter less than Compose's own advantages here: it's a plain, widely understood format that any contributor (or another tool) can read without knowing Aspire, it doesn't require a .NET-specific hosting model just to start a database, and there's no `AppHost`/`TestAppHost` project pair to keep in sync with the rest of the solution.

### Losing programmatic test orchestration is an accepted cost

Aspire's testing APIs (`DistributedApplicationTestingBuilder`) gave `dotnet test` a self-contained database with zero manual steps. Under Compose, that becomes a manual (or CI-scripted) `docker compose up -d` before running tests. This is a real regression in "it just works," accepted in exchange for the simplicity above — and it's the same tradeoff every non-Aspire .NET codebase already makes.

## Consequences

**Easier:**
- One `docker-compose.yml`, in a format every contributor already knows, replaces two hosting projects (`AppHost`, `TestAppHost`) and their package dependencies.
- Adding another infrastructure dependency (Redis, MinIO, ...) is a new service block in the compose file, not a new Aspire resource plus a code change to wire it up.
- No Aspire dashboard, no Aspire-specific launch profiles, no coupling to Aspire's release cadence.

**Harder:**
- `docker compose up -d` is now a manual prerequisite for running `Application.FunctionalTests` and `Web.AcceptanceTests`, both locally and in CI — there is no more self-contained `dotnet test`.
- `Web.AcceptanceTests` serves the frontend from the Web app's own `wwwroot`, so the SPA must be built (`npm run build` in `ClientApp`/`ClientApp-React`) before running acceptance tests. This is a behavior change from before, when Aspire ran the frontend's live dev server.
- No dashboard, distributed tracing view, or resource health view for local development; OpenTelemetry export still works if an OTLP endpoint is configured, but there's no bundled place to view it.
