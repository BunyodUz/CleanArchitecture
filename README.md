# Clean Architecture Solution Template

[![Build](https://github.com/jasontaylordev/CleanArchitecture/actions/workflows/build.yml/badge.svg)](https://github.com/jasontaylordev/CleanArchitecture/actions/workflows/build.yml)
[![CodeQL](https://github.com/jasontaylordev/CleanArchitecture/actions/workflows/codeql.yml/badge.svg)](https://github.com/jasontaylordev/CleanArchitecture/actions/workflows/codeql.yml)
[![Nuget](https://img.shields.io/nuget/v/Clean.Architecture.Solution.Template?label=NuGet)](https://www.nuget.org/packages/Clean.Architecture.Solution.Template)
[![Nuget](https://img.shields.io/nuget/dt/Clean.Architecture.Solution.Template?label=Downloads)](https://www.nuget.org/packages/Clean.Architecture.Solution.Template)
![Twitter Follow](https://img.shields.io/twitter/follow/jasontaylordev?label=Follow&style=social)

The goal of this template is to provide a straightforward and efficient approach to enterprise application development, leveraging the power of Clean Architecture and ASP.NET Core. Using this template, you can effortlessly create a new app with Angular, React, or Web API only, powered by ASP.NET Core. Getting started is easy - simply install the **.NET template** (see below for full details).

For full documentation, visit **[cleanarchitecture.jasontaylor.dev](https://cleanarchitecture.jasontaylor.dev)**.

If you find this project useful, please give it a star. Thanks! ⭐

## Getting started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- [Node.js](https://nodejs.org/) (LTS) — only required if you plan to use the Angular or React frontend
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) or [Podman](https://podman.io/) (or any OCI-compliant container runtime) with [Docker Compose](https://docs.docker.com/compose/) — required to run PostgreSQL locally.

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

Start the infrastructure dependencies (PostgreSQL):

```bash
docker compose up -d
```

Then run the Web API:

```bash
dotnet run --project src/Web
```

If you're using the Angular or React frontend, run its dev server separately:

```bash
cd src/Web/ClientApp   # or ClientApp-React
npm install
npm start
```

To learn more, see the [Getting started](https://cleanarchitecture.jasontaylor.dev/docs/getting-started/) guide and [Architecture](https://cleanarchitecture.jasontaylor.dev/docs/architecture/) overview.

### Running tests

`Application.FunctionalTests` and `Web.AcceptanceTests` connect to PostgreSQL rather than provisioning it themselves, so start it first:

```bash
docker compose up -d
```

`Web.AcceptanceTests` also serves the frontend from the Web app's own build output, so build it once beforehand:

```bash
cd src/Web/ClientApp   # or ClientApp-React
npm install
npm run build
```

Then run the tests as usual, e.g. `dotnet test`.

## Technologies

* [ASP.NET Core 10](https://docs.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core)
* [Docker Compose](https://docs.docker.com/compose/) — local infrastructure (see [ADR-005](docs/decisions/ADR-005-Docker-Compose-Instead-Of-Aspire.md))
* [Entity Framework Core 10](https://docs.microsoft.com/en-us/ef/core/)
* [Angular 21](https://angular.dev/) or [React 19](https://react.dev/)
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
