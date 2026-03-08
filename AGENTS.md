# AGENTS.md

## Cursor Cloud specific instructions

### Overview

This is a .NET 9 e-commerce backend API (ASP.NET Core) with JWT auth, SQLite (dev), Redis (optional), and Swagger docs. Single-project solution — no monorepo, no frontend, no test projects.

### Running the application

```bash
ASPNETCORE_ENVIRONMENT=Development DOTNET_URLS="http://0.0.0.0:5000" dotnet run
```

- Swagger UI: `http://localhost:5000/swagger`
- Health check: `http://localhost:5000/health`
- API routes use singular controller names (e.g. `/api/Product`, `/api/Auth/login`, `/api/Cart`)

### Key gotchas

- **Missing SQLite NuGet package**: The `EcommerceBackend.csproj` does not include `Microsoft.EntityFrameworkCore.Sqlite`, but `Program.cs` calls `UseSqlite()`. You must add it: `dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 9.0.9`. The update script handles this automatically.
- **No Redis required**: The app registers Redis caching in DI, but gracefully falls back to in-memory cache if Redis is unavailable. You do not need to run Redis for development.
- **Auto-seeding**: On first startup the app creates the SQLite database (`ecommerce.db`) and seeds ~1700 products, categories, users, campaigns, and reviews. The `ecommerce.db` file is created in the project root.
- **HealthChecks UI error**: A non-blocking `HttpRequestException` about `0.0.0.0` appears in logs from the HealthChecks UI background service. It does not affect API functionality.
- **No test projects**: `dotnet test` finds nothing to run. The solution only contains `EcommerceBackend.csproj`.

### Build and lint

Standard `dotnet build` from the workspace root. There is no separate linter configured — build warnings serve as the lint check. See `README.md` for general development docs.
