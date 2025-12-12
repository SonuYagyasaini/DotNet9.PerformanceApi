# DotNet9 Performance API - Final Combined

This bundle contains the full production-ready solution:
- Api (Minimal API) using .NET 9 with Swagger and optional JWT
- Worker (BackgroundService) that reads from Azure Service Bus and bulk inserts using SqlBulkCopy
- Infrastructure project with AppDbContext and EF Core migrations
- Domain + Application projects
- EF Core Migrations (InitialCreate) and snapshot
- SQL scripts and migration runner scripts (PowerShell / Bash)
- Dockerfile and docker-compose
- GitHub Actions workflow
- Azure Bicep template for SQL & Service Bus

## Quickstart
1. Extract to `D:\DotNet9.PerformanceApi` (recommended for stable paths).
2. Open `DotNet9.PerformanceApi.sln` in Visual Studio 2026.
3. Update `src/Api/appsettings.json` with SQL & Service Bus connection strings.
4. Run migrations:
   - Install dotnet-ef: `dotnet tool install --global dotnet-ef --version 9.0.0`
   - Run: `scripts/run-migrations.ps1` (Windows) or `scripts/run-migrations.sh` (Linux/Mac)
5. Start Api (Swagger at /swagger) and Worker.

