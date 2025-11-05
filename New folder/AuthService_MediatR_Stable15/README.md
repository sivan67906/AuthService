# AuthService (Vertical Slice + CQRS + Dual DB + MediatR + Aspire)

A .NET 9 WebAPI microservice for Auth (Register/Login/2FA/External, JWT/Refresh, Roles) using:
- Vertical Slice (feature folders) & CQRS (MediatR)
- Dual EF Core DbContexts: **SQL Server for commands** (Identity + writes) and **PostgreSQL for queries**
- ASP.NET Core Identity + JWT + Refresh tokens
- 2FA (Authenticator), External auth (Google/Microsoft)
- FluentValidation, Mapster, Serilog, HealthChecks
- Repository pattern (no UoW)
- xUnit tests
- .NET Aspire orchestration
- Docker & docker-compose

## Run locally (no containers)
```bash
dotnet restore
dotnet build
dotnet run --project src/AuthService.Api
```

Configure connection strings and JWT in `src/AuthService.Api/appsettings.json`.

## Run with Docker
```bash
docker compose up --build
```
- API at http://localhost:8080
- SQL Server on 1433, Postgres on 5432

## Run with Aspire
```bash
dotnet run --project orchestration/AuthService.AppHost
```
Wire the generated connection strings into `appsettings.json` or environment variables as needed.

## Accounts
- Seeds roles **Admin**/**User** and an admin account `admin@demo.local` / `Admin@12345`.

## MediatR Pipeline Behaviors
- **ValidationBehavior**: runs FluentValidation on all requests.
- **LoggingBehavior**: logs request handling + duration.
