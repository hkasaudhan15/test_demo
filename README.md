# CleanArch API

Production-grade ASP.NET Core 8 Web API template built with **Clean Architecture**, **CQRS**, and **Domain-Driven Design** patterns.

## Architecture

```
src/
  Core/
    Domain/           Pure domain model (entities, value objects, events, specifications)
    Application/      Use cases (commands, queries, validators, behaviors)
    SharedKernel/     Cross-cutting primitives (pagination, constants, extensions)
  External/
    Infrastructure/   EF Core, Redis, JWT, Dapper, health checks
    Presentation/     ASP.NET controllers, middleware, Swagger, CORS
    Migrator/         Standalone EF Core migration runner
  CrossCutting/       Outbox pattern, idempotency, multi-tenancy
tests/
  Unit/               Domain + Application unit tests
  Integration/        WebApplicationFactory-based integration tests
  Architecture/       NetArchTest layer dependency enforcement
```

### Dependency Rule

```
Domain  <--  Application  <--  Infrastructure
                                    |
                              Presentation
```

Domain has zero external dependencies (only MediatR.Contracts for event interfaces).  
Application depends only on Domain + abstractions.  
Infrastructure and Presentation implement the abstractions.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (local or Docker)
- Redis (optional, gracefully degrades)

## Quick Start

```bash
# Restore and build
dotnet restore
dotnet build

# Run with Docker (SQL Server + Redis + Seq)
docker compose -f deploy/docker/docker-compose.yml up -d

# Run the API
dotnet run --project src/External/Presentation

# Run migrations
dotnet run --project src/External/Migrator -- --migrate

# Run tests
dotnet test
```

The API starts at `https://localhost:5001` (or `http://localhost:5000`).  
Swagger UI is available at the root URL in Development/Staging environments.

## Key Patterns

| Pattern | Implementation |
|---|---|
| CQRS | MediatR commands/queries with `ICommand<T>` / `IQuery<T>` |
| Result Pattern | `Result<T>` instead of exceptions for expected failures |
| Outbox Pattern | Domain events saved atomically, processed by background service |
| Optimistic Concurrency | SQL Server `rowversion` on all aggregate roots |
| Soft Delete | Automatic via EF Core interceptor + global query filter |
| Audit Trail | `CreatedOnUtc`, `CreatedBy`, `ModifiedOnUtc`, `ModifiedBy` auto-stamped |
| Specification Pattern | Composable domain query logic (And/Or/Not) |
| Pipeline Behaviors | Logging, validation, performance monitoring via MediatR |

## Configuration

Configuration is in `appsettings.json`:

| Key | Description |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `ConnectionStrings:Redis` | Redis connection string |
| `JwtSettings:SecretKey` | JWT signing key (min 32 chars) |
| `JwtSettings:Issuer` | Token issuer |
| `JwtSettings:Audience` | Token audience |
| `Cors:AllowedOrigins` | Allowed CORS origins |

## Adding a New Feature

1. Copy `src/Core/Application/Features/_Template/` to your feature name
2. Define your entity in `Domain/` as an `AggregateRoot`
3. Create command/query handlers following the template structure
4. Add FluentValidation validators
5. Create a controller in `Presentation/Api/Controllers/`
6. Register the entity's `DbSet` in `ApplicationDbContext`
7. Add EF Core configuration extending `AggregateRootConfiguration<T>`

## Testing

```bash
dotnet test                          # All tests
dotnet test tests/Unit               # Unit tests only
dotnet test tests/Architecture       # Architecture enforcement
dotnet test tests/Integration        # Integration tests (needs DB)
```

## Docker

```bash
docker compose -f deploy/docker/docker-compose.yml up --build
```

Services: API (:8080), SQL Server (:1433), Redis (:6379), Seq (:5341)
