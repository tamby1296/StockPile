# StockPile

E-commerce backend with inventory management, built with ASP.NET Core on .NET 10.

## Tech stack

| Concern        | Choice                                           |
| -------------- | ------------------------------------------------ |
| Runtime        | .NET 10 (LTS), C# latest, nullable enabled       |
| Web            | ASP.NET Core Web API with controllers            |
| Database       | PostgreSQL 17 via Npgsql                         |
| Data access    | EF Core 10 (migrations owned by Infrastructure)  |
| Validation     | FluentValidation                                 |
| Logging        | Serilog (console sink, configured from settings) |
| Tests          | xUnit, `WebApplicationFactory`, NetArchTest      |

Package versions are managed centrally in [`Directory.Packages.props`](Directory.Packages.props); shared build
settings (nullable, warnings as errors, analyzers) live in [`Directory.Build.props`](Directory.Build.props).

## Solution layout

```
StockPile.sln
src/
  StockPile.Api             controllers, middleware, composition root (Program.cs)
  StockPile.Application     services, use cases, validators, abstractions (interfaces)
  StockPile.Domain          entities, enums, value objects, state machine rules
  StockPile.Infrastructure  EF Core DbContext, SQL, Stripe, implementations of Application interfaces
tests/
  StockPile.Api.Tests            integration / smoke tests via an in-memory test host
  StockPile.ArchitectureTests    layering rules enforced with NetArchTest
```

### Layering rules

```
Api ──► Application ──► Domain
 │            ▲
 └──► Infrastructure (implements Application interfaces)
```

- **Domain** depends on nothing else in the solution and on no framework (no EF Core, ASP.NET Core or Npgsql).
- **Application** depends only on Domain. It defines the interfaces (repositories, payment gateway, clock, ...)
  that Infrastructure implements.
- **Infrastructure** depends on Application (and transitively Domain), never on Api.
- **Api** is the composition root: it wires Application and Infrastructure together in `Program.cs`.
- **Controllers** only parse the request, validate it, call **one** application service and map the result or
  error to an HTTP status code.
- **Services** own business rules, transactions and authorization checks.

These rules are checked by `tests/StockPile.ArchitectureTests`; a violation fails the build pipeline.

## Agreed product decisions (v1)

- **Tax**: a flat percentage per product, stored as an integer in **basis points** (e.g. `825` = 8.25 %).
- **Currency**: the store operates in **one currency**; amounts are stored as integer minor units.
- **Variants**: product variants (size, colour, ...) are **out of scope for v1**.

## Prerequisites

- [.NET SDK 10.0.100+](https://dotnet.microsoft.com/download) (pinned with roll-forward in [`global.json`](global.json))
- PostgreSQL 17, only needed once features touch the database. `/health` and the test suite run without it.

## Build

```bash
dotnet build StockPile.sln
```

The build treats every compiler, analyzer and MSBuild warning as an error, so a clean build means zero warnings.

## Run

```bash
dotnet run --project src/StockPile.Api
```

Then check the API is up:

```bash
curl http://localhost:5292/health   # -> 200 Healthy
```

The connection string is `ConnectionStrings:StockPile` in `src/StockPile.Api/appsettings.json`. Override it
without editing files with an environment variable or user secrets:

```bash
export ConnectionStrings__StockPile="Host=localhost;Port=5432;Database=stockpile;Username=...;Password=..."
# or
dotnet user-secrets --project src/StockPile.Api set "ConnectionStrings:StockPile" "Host=..."
```

## Test

```bash
dotnet test StockPile.sln
```

- `StockPile.Api.Tests` boots the API in a `WebApplicationFactory` test host and asserts `GET /health` returns 200.
- `StockPile.ArchitectureTests` fails if Domain references Application, Infrastructure or Api, if Application
  references Infrastructure or Api, or if Infrastructure references Api.
