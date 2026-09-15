# ADR-004: Numbered SQL Script Migrations with DbUp

## Status

Accepted

## Date

2026-09-15

## Context

The template previously had no real database migration strategy: `ApplicationDbContextInitialiser` called `Database.EnsureDeletedAsync()` followed by `Database.EnsureCreatedAsync()` on every startup, dropping and recreating the schema from the current EF Core model. This is convenient for a demo but unusable once a database holds real data — there is no way to evolve the schema without losing it, and no record of what changed or when.

This fork targets PostgreSQL exclusively (see the removal of the SQL Server/SQLite template conditionals) and needs a migration approach that is explicit, reviewable in diffs, and does not require the EF Core model and the database schema to be inferred from each other.

## Decision

Schema changes are expressed as plain, numbered SQL scripts under `src/Infrastructure/Data/Scripts` (e.g. `0001_InitialCreate.sql`, `0002_...sql`), applied in filename order by [DbUp](https://dbup.readthedocs.io/) on startup. DbUp tracks which scripts have run in a `SchemaVersions` journal table in the target database and only applies new ones. `ApplicationDbContextInitialiser.InitialiseAsync` runs the upgrade instead of calling `EnsureCreated`/`EnsureDeleted`.

Scripts are plain files on disk (copied to the output directory at build time), not embedded resources — they stay easy to open, diff, and run manually against a database if needed.

## Rationale

### EF Core Migrations were also an option, and were rejected

EF Core's own migration system (`dotnet ef migrations add`, `Database.Migrate()`) was the more obvious alternative, since the project already uses EF Core. It was rejected here because:

- Migrations are generated from the EF model diff, which produces correct but often unreadable SQL (verbose, provider-specific `migrationBuilder` calls) that is hard to review in a PR.
- It couples schema evolution to the ORM: any schema change not expressible through the EF model (e.g. a view, a partial index, a stored procedure, a data backfill with custom SQL) needs an escape hatch (`migrationBuilder.Sql(...)`) anyway.
- Teams with existing SQL conventions, DBA review processes, or a preference for hand-written DDL are better served by scripts they fully control than by generated migration classes.

Numbered SQL scripts make every change a plain `.sql` file: readable, diffable, and runnable outside of .NET entirely if needed.

### DbUp over a hand-rolled runner

Writing a bespoke "read files, track what ran, apply the rest" runner is a small amount of code, but DbUp already does this reliably — ordering, transactions, journaling, idempotency — and is the de facto standard for this exact pattern in .NET. Using it avoids re-solving a solved problem.

### Consequence: EF Core model changes and scripts must be kept in sync manually

Because the database schema is no longer generated from the EF model, a change to a `DbSet<T>` entity or its `IEntityTypeConfiguration<T>` (a new property, a new relationship) does **not** automatically produce a schema change. A new numbered script must be added by hand to keep the database in sync with the model. This is the deliberate tradeoff of this decision: schema changes become explicit, at the cost of no longer being derived automatically.

## Consequences

**Easier:**
- Every schema change is a small, reviewable `.sql` file with a clear place to add provider-specific DDL, data backfills, or non-EF-representable objects (views, indexes with `WHERE` clauses, etc.).
- The schema's history is the ordered list of scripts — no need to inspect EF's migration snapshot to understand what changed.
- Scripts can be handed to a DBA or run manually against a database outside of the application.

**Harder:**
- Entity/configuration changes require a matching hand-written script; nothing generates SQL from the EF model automatically.
- Scripts are PostgreSQL-specific SQL; this decision assumes a single target provider (see the removal of the SQL Server/SQLite conditionals from this fork).
- Once a script has shipped and run against any environment, it must never be edited — a mistake is fixed with a new script, the same discipline required by any migration tool.
