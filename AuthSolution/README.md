# AuthSolution (.NET 9) — Vertical Slice + CQRS + MediatR + Dual DB (SQL Server / PostgreSQL)

**Highlights**
- Vertical Slice feature folders w/ MediatR (Commands on SQL Server, Queries on PostgreSQL)
- Repository pattern (no UoW), ASP.NET Core Identity, JWT Access/Refresh
- Two-factor (TOTP) endpoints, External auth (Google/Microsoft) wiring
- FluentValidation, Mapster, Serilog, HealthChecks, HttpClientFactory
- EF Core 9 (preview), Fluent API mappings, seed data (Admin user + roles)
- Aspire-style `ServiceDefaults` + `Orchestrator` skeleton
- xUnit tests

## Quick start
1. Update connection strings in `src/AuthService.Api/appsettings.json`.
2. Ensure SQL Server & PostgreSQL are running.
3. From solution root:
   ```bash
   dotnet build
   dotnet run --project src/AuthService.Api
   ```
4. Hit `GET /api/health` to verify.

> Note: Packages target .NET 9 previews. If your SDK differs, bump versions appropriately.
