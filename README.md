# Employee 360 — REST API

Employee 360 is an employee management application built with **ASP.NET Core Web API, C#, Angular, and SQL Server**. It focuses on secure API access, maintainable application architecture, and reliable employee workflows.

## Features

- **JWT authentication** with access-token and refresh-token management
- **API rate limiting** to help protect endpoints from excessive requests
- **Employee authentication** and attendance punch-in functionality
- **Entity Framework Core** for data access
- **Serilog** for structured application logging
- **Hangfire** for background job scheduling
- Angular frontend integrated with the ASP.NET Core API
- Swagger/OpenAPI support for exploring API endpoints

## Technology stack

| Area | Technologies |
|---|---|
| Backend | C#, ASP.NET Core Web API |
| Frontend | Angular |
| Database | Microsoft SQL Server |
| ORM | Entity Framework Core |
| Authentication | JWT, refresh tokens |
| API protection | Rate limiting |
| Logging | Serilog |
| Background jobs | Hangfire |
| API documentation | Swagger / OpenAPI |

## Repository structure

| Path | Purpose |
|---|---|
| `Rest_Api/` | ASP.NET Core Web API |
| `Services/` | Business logic and services |
| `Repository/` | Data access layer |
| `Core/` | Entity Framework Core entities and DbContext |
| `Models/` | Shared models and DTOs |
| `StartAngular/` | Angular frontend |
| `scripts/` | SQL scripts |

## Prerequisites

Install the following before running the project:

- .NET SDK compatible with the API project
- Node.js and npm
- Angular CLI version compatible with the frontend
- Microsoft SQL Server
- Visual Studio or Visual Studio Code (recommended)

## Run the backend

1. Configure the database connection string and other required settings using your local development configuration.
2. From the repository root, run:

   ```bash
   dotnet restore Rest_Api/Rest_Api.csproj
   dotnet run --project Rest_Api/Rest_Api.csproj
   ```

3. Open the Swagger URL shown in the application output, commonly `https://localhost:<port>/swagger`.

## Run the Angular frontend

1. Open a terminal in the frontend directory:

   ```bash
   cd StartAngular
   npm install
   npm start
   ```

2. The Angular development server commonly runs at `http://localhost:4200`.

Make sure the API URL and CORS configuration match your local development environment.

## Authentication flow

1. Send credentials to `POST /api/login/validate` to obtain an access token and refresh token.
2. Send the access token in the request header: `Authorization: Bearer <token>`.
3. When the access token expires, use `POST /api/login/refresh` to request a refreshed token.
4. Sign out through `POST /api/login/logout`.

Check the API implementation and Swagger documentation for the exact request and response contracts.

## Security notes

- Never commit real passwords, database connection strings, JWT signing keys, API keys, or other secrets.
- Keep environment-specific values in local configuration or a secret manager.
- Use HTTPS outside local development and configure token expiry and refresh-token validation appropriately.

## Purpose

This project demonstrates full-stack employee application development, secure REST API design, authentication, logging, background processing, and SQL Server integration.
