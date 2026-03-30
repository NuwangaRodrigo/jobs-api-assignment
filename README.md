# Job Processing API

A personal project built with ASP.NET Core to explore background job processing, clean architecture, and REST API design patterns.

## Overview

This API handles processing of large item sets in the background. Clients submit a job, get an ID back immediately, and can poll for status while processing happens asynchronously.

Two processing modes are supported:

- **Bulk** — process all items sequentially, continue even if some fail
- **Batch** — process items in order, stop immediately on first failure

## Architecture

Built with a layered approach to keep concerns separated and the code testable:

```
API Layer          → Controllers, Middleware
Application Layer  → Services, Strategies
Core Layer         → Domain models, Interfaces
Infrastructure     → Data access, External services
```

### Design patterns

**Strategy Pattern** — Bulk and Batch processing implement the same interface, making it easy to add new job types without touching existing code.

**Repository Pattern** — Data access is abstracted from business logic. Currently in-memory, but swappable without changing any business logic.

**Factory Pattern** — Centralizes strategy selection logic.

**Dependency Injection** — Used throughout for loose coupling and testability.

## Getting started

You'll need .NET 8.0 SDK installed.

```bash
git clone https://github.com/NuwangaRodrigo/jobs-api-assignment.git
cd jobs-api-assignment/JobProcessingApi

dotnet restore
dotnet build
dotnet test

cd src/JobProcessingApi.API
dotnet run
```

Swagger UI will be available at the URL shown in the console.

## Authentication

The API uses JWT tokens for securing endpoints.

1. POST to `/api/auth/token` with `{"username": "test-user"}`
2. Copy the token from the response
3. In Swagger, click **Authorize** and enter: `Bearer YOUR_TOKEN`

For local testing you can disable auth by commenting out `[Authorize]` in `JobsController.cs`.

## API endpoints

### Start a job
```http
POST /api/jobs
Content-Type: application/json

{
  "jobType": 0,
  "items": ["item1", "item2", "item3"]
}
```

`jobType`: `0` = Bulk, `1` = Batch

Returns `202 Accepted` with a `jobId`. Processing starts immediately in the background.

### Check status
```http
GET /api/jobs/{jobId}/status
```

Returns progress, item counts, and current status:
- `0` = Pending
- `1` = Running
- `2` = Completed
- `3` = Failed
- `4` = PartiallyCompleted

### Get logs
```http
GET /api/jobs/{jobId}/logs
```

Returns detailed processing results for each item.

## How Bulk vs Batch behaves

**Bulk:**
```
Item 1 → Success ✅
Item 2 → Failure ❌
Item 3 → Success ✅  (keeps going)
Result: PartiallyCompleted
```

**Batch:**
```
Item 1 → Success ✅
Item 2 → Failure ❌ (stops here)
Item 3 → Skipped
Result: Failed
```

## Testing

```bash
dotnet test
```

All 17 tests should pass. Test coverage includes processing strategies, service logic, and repository behaviour.

**Testing tools:** xUnit, Moq, FluentAssertions

## Project structure

```
src/
  JobProcessingApi.API/            Controllers, middleware, config
  JobProcessingApi.Core/           Domain models and interfaces
  JobProcessingApi.Application/    Business logic and strategies
  JobProcessingApi.Infrastructure/ Data access
tests/
  JobProcessingApi.Tests/          Unit and integration tests
```

## Data storage

Uses in-memory storage (`ConcurrentDictionary`) to keep the setup simple. The repository pattern makes swapping to a real database straightforward — just implement `IJobRepository` and update the DI registration. No changes needed to business logic.

## Logging

Structured logging with Serilog. Writes to console and a daily rolling file at `logs/jobprocessing-YYYYMMDD.log`.

## Technologies

- .NET 8 / ASP.NET Core / C#
- Serilog
- FluentValidation
- Swagger / OpenAPI
- JWT Bearer Authentication
- xUnit / Moq / FluentAssertions

## SOLID principles applied

- **S** — Each class has a single reason to change
- **O** — New job types can be added without modifying existing strategies
- **L** — All strategies are interchangeable through a shared interface
- **I** — Interfaces are small and focused
- **D** — Dependencies are on abstractions, not concrete implementations

---

*Built by Nuwanga Rodrigo*
