# Lisan

AI-powered Persian/Farsi language learning platform for children in diaspora families.

## Repository Structure

```
backend/          .NET 8 ASP.NET Core backend
  src/
    Lisan.Domain/          # Entities, value objects, domain events
    Lisan.Application/     # Use cases, interfaces, DTOs
    Lisan.Infrastructure/  # EF Core, external services, repositories
    Lisan.Api/             # Minimal API endpoints, middleware, DI root
  tests/
    Lisan.Tests/           # xUnit unit and integration tests
mobile/           React Native Expo mobile app (coming soon)
scripts/          Shared automation scripts
docs/             Architecture and API documentation
```

## Backend

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- PostgreSQL 15+

### Getting Started

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run tests
dotnet test

# Run API (development)
dotnet run --project backend/src/Lisan.Api
```

The API starts on `https://localhost:7000` by default.

### Database Migrations

```bash
# Add a migration
dotnet ef migrations add <MigrationName> --project backend/src/Lisan.Infrastructure --startup-project backend/src/Lisan.Api

# Apply migrations
dotnet ef database update --project backend/src/Lisan.Infrastructure --startup-project backend/src/Lisan.Api
```

### Environment Variables

| Variable | Description |
|----------|-------------|
| `ConnectionStrings__Default` | PostgreSQL connection string |
| `ASPNETCORE_ENVIRONMENT` | `Development` / `Production` |

## Mobile

### Prerequisites

- [Node.js 20+](https://nodejs.org/)
- [Expo CLI](https://docs.expo.dev/get-started/installation/) (`npm install -g expo-cli`)

### Getting Started

```bash
cd mobile

# Install dependencies
npm install

# Start Expo dev server
npx expo start

# Run on iOS simulator
npx expo run:ios

# Run on Android emulator
npx expo run:android
```
