# Clean Architecture Solution Template

[![Build](https://github.com/jasontaylordev/CleanArchitecture/actions/workflows/build.yml/badge.svg)](https://github.com/jasontaylordev/CleanArchitecture/actions/workflows/build.yml)
[![CodeQL](https://github.com/jasontaylordev/CleanArchitecture/actions/workflows/codeql.yml/badge.svg)](https://github.com/jasontaylordev/CleanArchitecture/actions/workflows/codeql.yml)
[![Nuget](https://img.shields.io/nuget/v/Clean.Architecture.Solution.Template?label=NuGet)](https://www.nuget.org/packages/Clean.Architecture.Solution.Template)
[![Nuget](https://img.shields.io/nuget/dt/Clean.Architecture.Solution.Template?label=Downloads)](https://www.nuget.org/packages/Clean.Architecture.Solution.Template)
![Twitter Follow](https://img.shields.io/twitter/follow/jasontaylordev?label=Follow&style=social)

The goal of this template is to provide a straightforward and efficient approach to enterprise application development, leveraging the power of Clean Architecture and ASP.NET Core. Using this template, you can effortlessly create a new app with Angular, React, or Web API only, powered by ASP.NET Core and Aspire. Getting started is easy - simply install the **.NET template** (see below for full details).

For full documentation, visit **[cleanarchitecture.jasontaylor.dev](https://cleanarchitecture.jasontaylor.dev)**.

If you find this project useful, please give it a star. Thanks! ⭐

## Getting started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- [Node.js](https://nodejs.org/) (LTS) — required for the Next.js frontend
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) or [Podman](https://podman.io/) (or any OCI-compliant container runtime) — required to run PostgreSQL locally via Aspire.

### Install the template

```bash
dotnet new install Clean.Architecture.Solution.Template
```

### Create a new solution

Create a new solution using the template. Specify the client framework using `--client-framework` (`-cf`) and the database provider using `--database` (`-db`):

```bash
dotnet new ca-sln --client-framework [angular|react|none] --database [postgresql|sqlite|sqlserver] --output YourProjectName
```

| Option | Values | Default |
|---|---|---|
| `--client-framework` | `angular`, `react`, `none` | `angular` |
| `--database` | `postgresql`, `sqlite`, `sqlserver` | `sqlite` |

**Examples:**

🅰️ Angular SPA with ASP.NET Core Web API and PostgreSQL:
```bash
dotnet new ca-sln -cf angular -db postgresql -o YourProjectName
```

⚛️ React SPA with ASP.NET Core Web API and SQL Server:
```bash
dotnet new ca-sln -cf react -db sqlserver -o YourProjectName
```

🔌 ASP.NET Core Web API only with SQLite:
```bash
dotnet new ca-sln -cf none -db sqlite -o YourProjectName
```

> 💡 **Tip:** Run `dotnet new ca-sln --help` to see all available template options.

### Run the app

```bash
dotnet run --project .\src\AppHost
```

The Aspire dashboard will open automatically, showing the application URLs and logs. This also starts a local Keycloak instance (see [ADR-005](docs/decisions/ADR-005-Keycloak-Authentication-And-Permission-Based-Authorization.md)) with a realm imported from `deploy/keycloak/realm-export.json`. Sign in with the seeded dev users:

| Username | Password | Notes |
|---|---|---|
| `administrator@localhost` | `Administrator1!` | Has the `Administrator` role and all todo permissions |
| `member@localhost` | `Member1!` | Has the todo permissions only |

These are local-dev-only accounts defined in the realm export — change or remove them for anything beyond local development.

To learn more, see the [Getting started](https://cleanarchitecture.jasontaylor.dev/docs/getting-started/) guide and [Architecture](https://cleanarchitecture.jasontaylor.dev/docs/architecture/) overview.

## Technologies

* [ASP.NET Core 10](https://docs.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core)
* [Aspire](https://aspire.dev)
* [Keycloak](https://www.keycloak.org/) — authentication via the BFF pattern, permission-based authorization (see [ADR-005](docs/decisions/ADR-005-Keycloak-Authentication-And-Permission-Based-Authorization.md))
* [Entity Framework Core 10](https://docs.microsoft.com/en-us/ef/core/)
* [Next.js 16](https://nextjs.org/) (static export) with [React 19](https://react.dev/), [Mantine](https://mantine.dev/) and [Effector](https://effector.dev/), organized using [Feature-Sliced Design](https://feature-sliced.design/)
* [MediatR](https://github.com/jbogard/MediatR)
* [AutoMapper](https://automapper.org/)
* [FluentValidation](https://fluentvalidation.net/)
* [NUnit](https://nunit.org/), [Shouldly](https://docs.shouldly.org/), [Moq](https://github.com/devlooped/moq) & [Respawn](https://github.com/jbogard/Respawn)
* [DbUp](https://dbup.readthedocs.io/) — numbered SQL script migrations (see [ADR-004](docs/decisions/ADR-004-SQL-Script-Migrations-With-DbUp.md))
* [Scalar](https://scalar.com/)

## Versions

The `main` branch is on **.NET 10.0**. Previous versions are available:

| Version | Branch |
|---|---|
| .NET 9.0 | [`net9.0`](https://github.com/jasontaylordev/CleanArchitecture/tree/net9.0) |
| .NET 8.0 | [`net8.0`](https://github.com/jasontaylordev/CleanArchitecture/tree/net8.0) |
| .NET 7.0 | [`net7.0`](https://github.com/jasontaylordev/CleanArchitecture/tree/net7.0) |
| .NET 6.0 | [`net6.0`](https://github.com/jasontaylordev/CleanArchitecture/tree/net6.0) |
| .NET 5.0 | [`net5.0`](https://github.com/jasontaylordev/CleanArchitecture/tree/net5.0) |
| .NET Core 3.1 | [`netcore3.1`](https://github.com/jasontaylordev/CleanArchitecture/tree/netcore3.1) |

## Architectural decisions

Key design decisions are documented as [Architecture Decision Records](docs/decisions/).

## Learn more

- 📖 [Clean Architecture Solution Template documentation](https://cleanarchitecture.jasontaylor.dev)

## Support

If you are having problems, please let me know by [raising a new issue](https://github.com/jasontaylordev/CleanArchitecture/issues/new/choose).

## License

This project is licensed under the [MIT License](LICENSE).
