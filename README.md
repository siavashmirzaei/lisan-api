# Lisan API

ASP.NET Core 8 backend for the Lisan heritage language learning platform.

## Architecture

Clean architecture with four layers:

```
src/
  Lisan.Domain/          # Entities, value objects, domain events
  Lisan.Application/     # Use cases, interfaces, DTOs
  Lisan.Infrastructure/  # EF Core, external services, repositories
  Lisan.Api/             # Controllers, middleware, DI composition root
tests/
  Lisan.Tests/           # xUnit unit tests
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- PostgreSQL 15+

## Getting Started

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run tests
dotnet test

# Run API (development)
dotnet run --project src/Lisan.Api
```

The API starts on `https://localhost:7000` by default.

## Database Migrations

```bash
# Add a migration
dotnet ef migrations add <MigrationName> --project src/Lisan.Infrastructure --startup-project src/Lisan.Api

# Apply migrations
dotnet ef database update --project src/Lisan.Infrastructure --startup-project src/Lisan.Api
```

## Environment Variables

| Variable | Description |
|----------|-------------|
| `ConnectionStrings__Default` | PostgreSQL connection string |
| `ASPNETCORE_ENVIRONMENT` | `Development` / `Production` |
