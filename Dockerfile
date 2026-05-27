# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy project files first for layer cache on restore
COPY Lisan.sln ./
COPY backend/src/Lisan.Api/Lisan.Api.csproj backend/src/Lisan.Api/
COPY backend/src/Lisan.Application/Lisan.Application.csproj backend/src/Lisan.Application/
COPY backend/src/Lisan.Domain/Lisan.Domain.csproj backend/src/Lisan.Domain/
COPY backend/src/Lisan.Infrastructure/Lisan.Infrastructure.csproj backend/src/Lisan.Infrastructure/
COPY backend/tests/Lisan.Tests/Lisan.Tests.csproj backend/tests/Lisan.Tests/

RUN dotnet restore

COPY backend/src/ backend/src/

RUN dotnet publish backend/src/Lisan.Api/Lisan.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# ASP.NET Core 8 defaults to port 8080 in containers
EXPOSE 8080

ENTRYPOINT ["dotnet", "Lisan.Api.dll"]
