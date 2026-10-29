# Employee Management

Monorepo with ASP.NET Core API + Angular UI.

## Structure

| Path | What |
|------|------|
| `Rest_Api/` | ASP.NET Core Web API (JWT + refresh tokens) |
| `Services/` | Business logic |
| `Repository/` | Data access |
| `Core/` | EF Core entities + DbContext |
| `Models/` | Shared DTOs |
| `StartAngular/` | Angular 16 frontend |
| `scripts/` | SQL scripts |

## Backend

```bash
dotnet run --project Rest_Api/Rest_Api.csproj
```

Swagger: `https://localhost:<port>/swagger`

## Frontend

```bash
cd StartAngular
npm install
npm start
```

Angular: `http://localhost:4200`

## Auth flow (short)

1. `POST /api/login/validate` → `token` + `refreshToken`
2. Call APIs with `Authorization: Bearer {token}`
3. When access token expires → `POST /api/login/refresh`
4. Logout → `POST /api/login/logout`
