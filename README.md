# Job Processing API

ASP.NET Core REST API for processing bulk and batch jobs with status tracking and logging.

## What it does

Processes jobs containing multiple items. Supports two modes:

- **Bulk**: Process all items sequentially, continue even if some fail
- **Batch**: Process items in order, stop on first failure

Jobs run asynchronously in the background. Clients can submit jobs and poll for status updates.

## Architecture

Layered architecture separating concerns:
```
API Layer          → Controllers, Middleware
Application Layer  → Services, Strategies
Core Layer         → Domain models, Interfaces  
Infrastructure     → Data access, External services
```

**Design patterns used:**

**Strategy Pattern** - Different processing behaviors (Bulk vs Batch) implement the same interface. Makes adding new job types straightforward.

**Repository Pattern** - Abstracts data access from business logic. Currently uses in-memory storage, but designed to swap implementations easily.

**Factory Pattern** - Centralizes strategy selection.

**Dependency Injection** - Loose coupling throughout for testability.

## Running locally

Prerequisites: .NET 8.0 SDK
```bash
git clone https://github.com/NuwangaRodrigo/jobs-api-assignment.git
cd jobs-api-assignment/JobProcessingApi

dotnet restore
dotnet build
dotnet test
cd src/JobProcessingApi.API
dotnet run
```

Swagger UI will be available at the URL shown in console (usually https://localhost:XXXX).

## Authentication

Uses JWT tokens. To test endpoints:

1. POST to /api/auth/token with `{"username": "test-user"}`
2. Copy the token from response
3. In Swagger, click Authorize and enter: `Bearer YOUR_TOKEN`

For local testing, you can disable auth by commenting out `[Authorize]` in JobsController.cs

## API endpoints

**Start a job**
```
POST /api/jobs
{
  "jobType": 0,
  "items": ["item1", "item2", "item3"]
}
```
jobType: 0 = Bulk, 1 = Batch

Returns 202 Accepted with a jobId. Processing starts in the background.

**Check status**
```
GET /api/jobs/{jobId}/status
```

Shows progress, item counts, and current status (0=Pending, 1=Running, 2=Completed, 3=Failed, 4=PartiallyCompleted).

**Get logs**
```
GET /api/jobs/{jobId}/logs
```

Returns processing details for each item.

## Testing

`dotnet test` runs the full test suite. All 17 tests should pass.

Tests cover processing strategies, service logic, and repository behavior. Using xUnit with Moq for mocking and FluentAssertions.

## Data storage

Currently using in-memory storage (ConcurrentDictionary) to keep setup simple and focus on architecture.

The repository pattern makes switching to a database straightforward - just implement IJobRepository and update DI registration. Business logic stays the same.

## Project structure
```
src/
  JobProcessingApi.API/          Controllers, middleware, config
  JobProcessingApi.Core/         Domain models and interfaces
  JobProcessingApi.Application/  Business logic and strategies  
  JobProcessingApi.Infrastructure/ Data access
tests/
  JobProcessingApi.Tests/        Unit and integration tests
```

## How Bulk vs Batch differs

**Bulk:**
- Process item 1 → Success
- Process item 2 → Failure
- Process item 3 → Success (keeps going)
- Result: PartiallyCompleted

**Batch:**
- Process item 1 → Success  
- Process item 2 → Failure (stops)
- Item 3 not processed
- Result: Failed

## Logging

Serilog writes to console and logs/jobprocessing-YYYYMMDD.log

## Technologies

.NET 8, ASP.NET Core, C#, FluentValidation, Serilog, Swagger, JWT Bearer, xUnit, Moq, FluentAssertions

## SOLID principles

Each class has single responsibility. Strategy pattern allows extension without modification. Strategies are interchangeable. Interfaces are focused. Dependencies are on abstractions, not implementations.

## Assignment requirements

Covers all must-haves: solution compiles, proper .NET usage, clean code/SOLID, dependency injection, n-layer architecture, working job processing, unit tests.

Also includes: design patterns, structured logging, middleware, API docs, authentication.

---

Built for Ohpen Senior Backend Developer assignment.
