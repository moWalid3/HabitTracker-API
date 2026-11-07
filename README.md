# HabitTracker

HabitTracker is a secure, API-first habit management application built with ASP.NET Core. It allows authenticated users to create, organize, and manage personal habits, track progress toward targets, and group habits with reusable tags.

## Features

- User registration, login, and JWT access/refresh tokens
- Per-user habit management with create, read, update, partial update, and delete operations
- Binary and measurable habits with daily, weekly, or monthly frequencies
- Habit statuses, targets, milestones, completion timestamps, and archiving support
- Tag management and habit-to-tag relationships
- Search, filtering, sorting, pagination, and response field selection for habits
- Optional HATEOAS links in API responses
- Optional GitHub integration for storing and revoking a personal access token and retrieving the authenticated GitHub profile
- Validation, structured problem details, CORS, HTTPS redirection, and rate limiting
- OpenAPI and Scalar API documentation in the Development environment

## Technology Stack

- .NET 9 and ASP.NET Core Web API
- Entity Framework Core 9 with SQL Server
- ASP.NET Core Identity and JWT Bearer authentication
- FluentValidation
- xUnit and Coverlet for testing

## Getting Started

### Prerequisites

- .NET 9 SDK
- SQL Server or SQL Server Express
- Git

### Run locally

1. Clone the repository and move into the project directory:

   ```bash
   git clone <repository-url>
   cd HabitTracker
   ```

2. Configure the database and JWT settings in `HabitTracker.Api/appsettings.json`, or override them with user secrets or environment variables. At minimum, configure:

   - `ConnectionStrings:Database`
   - `Jwt:Key`
   - `Jwt:Issuer`
   - `Jwt:Audience`

   Do not use a committed development key or production connection string for a deployed environment.

3. Apply the included Entity Framework migrations:

   ```bash
   dotnet ef database update --project HabitTracker.Api
   ```

4. Start the API:

   ```bash
   dotnet run --project HabitTracker.Api
   ```

   The default development URLs are:

   - `https://localhost:7127`
   - `http://localhost:5036`

5. In Development, open the Scalar API reference at `/scalar` or the OpenAPI document at `/openapi/v1.json`.

## API Overview

The API uses JWT bearer authentication for protected resources. Register or log in through `/Auth`, then send the returned access token in the `Authorization` header:

```text
Authorization: Bearer <access-token>
```

Main resource groups:

- `/Auth` — register, login, and refresh tokens
- `/Habits` — manage habits and query personal habits
- `/Tags` — manage personal tags
- `/habits/{habitId}/tags` — assign or remove tags from a habit
- `/Github` — manage a GitHub personal access token and retrieve the GitHub profile

All habit and tag data is scoped to the authenticated user.

## Development

Restore, build, and test the solution with:

```bash
dotnet restore
dotnet build
dotnet test
```

The solution contains:

- `HabitTracker.Api` — the ASP.NET Core Web API and database migrations
- `HabitTracker.UnitTests` — unit tests for services and validation

## License

No license has been specified for this repository yet.
