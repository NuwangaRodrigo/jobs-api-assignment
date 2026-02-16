# Job Processing API

A production-ready ASP.NET Core REST API for processing bulk and batch jobs with comprehensive logging, status tracking, and authentication.

Built for the Ohpen Senior Backend Code Assignment.

---

## 🎯 Overview

This API allows clients to process large volumes of data for migration scenarios. It provides:

- **Two processing strategies:**
  - **BULK**: Process all items sequentially, continue on failures
  - **BATCH**: Process items sequentially, stop on first failure

- **Async job execution:** Jobs run in the background, API responds immediately
- **Real-time status tracking:** Monitor progress, failures, and completion
- **Detailed audit logs:** View processing results for each item
- **JWT authentication:** Secure API access
- **Comprehensive error handling:** Global exception middleware

---

## 🏗️ Architecture

### Clean Architecture (N-Layer)

```
┌─────────────────────────────────────┐
│  API Layer                          │
│  Controllers, Middleware, Auth      │
└────────────┬────────────────────────┘
             ↓
┌─────────────────────────────────────┐
│  Application Layer                  │
│  Services, Strategies, Validators   │
└────────────┬────────────────────────┘
             ↓
┌─────────────────────────────────────┐
│  Core Layer                         │
│  Entities, Interfaces, DTOs         │
└─────────────────────────────────────┘
             ↑
┌─────────────────────────────────────┐
│  Infrastructure Layer               │
│  Repositories, External Services    │
└─────────────────────────────────────┘
```

### Design Patterns

**Strategy Pattern**
- Different algorithms (BULK vs BATCH) with same interface
- Easy to add new job types without modifying existing code
- Each strategy has single responsibility

**Repository Pattern**
- Abstracts data access from business logic
- Current: In-memory implementation (thread-safe with ConcurrentDictionary)
- Future: Easy to swap for SQL, NoSQL, or cloud storage

**Factory Pattern**
- Centralizes strategy selection logic
- Decouples strategy creation from usage

**Dependency Injection**
- Loose coupling throughout
- Highly testable with mocked dependencies

### SOLID Principles

✅ **Single Responsibility:** Each class has one reason to change  
✅ **Open/Closed:** Open for extension (new strategies), closed for modification  
✅ **Liskov Substitution:** All strategies are interchangeable  
✅ **Interface Segregation:** Small, focused interfaces  
✅ **Dependency Inversion:** Depend on abstractions, not implementations

---

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 / VS Code / Rider

### Setup

```bash
# Clone repository
git clone https://github.com/NuwangaRodrigo/jobs-api-assignment.git
cd jobs-api-assignment/JobProcessingApi

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests (all should pass)
dotnet test

# Run API
cd src/JobProcessingApi.API
dotnet run
```

The API will start at: `https://localhost:XXXX` (check console output)

Swagger UI: `https://localhost:XXXX`

---

## 🔐 Authentication

The API uses JWT authentication for all endpoints except token generation.

### Generate Test Token

**Option 1: Use Swagger UI**
1. Go to `POST /api/auth/token`
2. Click "Try it out"
3. Request body:
   ```json
   {
     "username": "test-user"
   }
   ```
4. Click "Execute"
5. Copy the token from response

**Option 2: Use cURL**
```bash
curl -X POST https://localhost:XXXX/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username":"test-user"}'
```

### Use Token in Requests

In Swagger:
1. Click "Authorize" button (top right)
2. Enter: `Bearer YOUR_TOKEN_HERE`
3. Click "Authorize"

In cURL:
```bash
curl -H "Authorization: Bearer YOUR_TOKEN_HERE" ...
```

**Note:** For development/testing, you can disable authentication by commenting out `[Authorize]` in `JobsController.cs`

---

## 📖 API Usage

### 1. Start a BULK Job

**Request:**
```http
POST /api/jobs
Content-Type: application/json
Authorization: Bearer YOUR_TOKEN

{
  "jobType": 0,
  "items": [
    "item-1-SUCCESS",
    "item-2-SUCCESS",
    "item-3-FAIL",
    "item-4-SUCCESS"
  ]
}
```

**Response (202 Accepted):**
```json
{
  "jobId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "message": "Job accepted and processing started"
}
```

**Behavior:**
- Processes all 4 items
- Item 3 fails but processing continues
- Final status: `PartiallyCompleted` (3/4 succeeded)

---

### 2. Start a BATCH Job

**Request:**
```json
{
  "jobType": 1,
  "items": [
    "item-1-SUCCESS",
    "item-2-FAIL",
    "item-3-SUCCESS"
  ]
}
```

**Behavior:**
- Processes items 1 and 2
- Stops at item 2 (failure)
- Item 3 is NOT processed
- Final status: `Failed` (1/3 succeeded, 1 failed, 1 skipped)

---

### 3. Check Job Status

**Request:**
```http
GET /api/jobs/{jobId}/status
Authorization: Bearer YOUR_TOKEN
```

**Response (200 OK):**
```json
{
  "jobId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "type": 0,
  "status": 4,
  "totalItems": 4,
  "processedItems": 4,
  "failedItems": 1,
  "successfulItems": 3,
  "progressPercentage": 100.0,
  "createdAt": "2025-02-16T10:00:00Z",
  "startedAt": "2025-02-16T10:00:01Z",
  "completedAt": "2025-02-16T10:00:05Z"
}
```

**Status Codes:**
- `0` = Pending
- `1` = Running
- `2` = Completed
- `3` = Failed
- `4` = PartiallyCompleted

---

### 4. Get Job Logs

**Request:**
```http
GET /api/jobs/{jobId}/logs
Authorization: Bearer YOUR_TOKEN
```

**Response (200 OK):**
```json
{
  "jobId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "logs": [
    {
      "itemIndex": 0,
      "itemData": "item-1-SUCCESS",
      "status": 0,
      "description": "Successfully processed item: item-1-SUCCESS",
      "processedAt": "2025-02-16T10:00:01Z",
      "processingTimeMs": 487
    },
    {
      "itemIndex": 2,
      "itemData": "item-3-FAIL",
      "status": 1,
      "description": "Processing failed for item: item-3-FAIL",
      "processedAt": "2025-02-16T10:00:03Z",
      "processingTimeMs": 512
    }
  ]
}
```

---

## 🧪 Testing

### Run All Tests
```bash
dotnet test
```

**Expected Output:**
```
Passed! - Failed: 0, Passed: 17, Skipped: 0
```

### Test Coverage

- **Strategy Tests:** BULK and BATCH processing behaviors
- **Service Tests:** Job orchestration and status tracking
- **Repository Tests:** Data access and thread safety
- **Integration Tests:** End-to-end flows

**Testing Tools:**
- xUnit (test framework)
- Moq (mocking)
- FluentAssertions (readable assertions)

---

## 💾 Data Persistence

**Current Implementation:** In-memory storage using `ConcurrentDictionary`

**Why In-Memory?**
- Assignment requirement: Focus on architecture, not database tech
- Fast development and testing
- Thread-safe for concurrent access
- Demonstrates repository pattern correctly

**Production Ready:**
The repository pattern makes it trivial to swap storage:

```csharp
// Current (In-Memory)
services.AddSingleton<IJobRepository, InMemoryJobRepository>();

// Future (SQL Database)
services.AddDbContext<JobDbContext>();
services.AddScoped<IJobRepository, EfCoreJobRepository>();

// Future (NoSQL)
services.AddScoped<IJobRepository, MongoJobRepository>();
```

No changes needed to business logic when swapping implementations.

---

## 📊 Logging

**Structured logging** with Serilog:

```
Console: Real-time output during development
File: logs/jobprocessing-YYYYMMDD.log (daily rolling)
```

**Log Levels:**
- `Debug`: Detailed item processing info
- `Information`: Job lifecycle events
- `Warning`: Item failures, validation issues
- `Error`: Unexpected exceptions

**Example log:**
```
[INF] Starting BULK job 3fa85f64 with 10 items
[WRN] Item 3 failed for job 3fa85f64: Validation error
[INF] BULK job 3fa85f64 completed. Processed: 10, Failed: 1
```

---

## 📦 Project Structure

```
JobProcessingApi/
├── src/
│   ├── JobProcessingApi.API/
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs      # JWT token generation
│   │   │   └── JobsController.cs      # Job endpoints
│   │   ├── Middleware/
│   │   │   ├── GlobalExceptionMiddleware.cs
│   │   │   └── RequestLoggingMiddleware.cs
│   │   ├── appsettings.json
│   │   └── Program.cs                 # DI configuration
│   │
│   ├── JobProcessingApi.Core/
│   │   ├── Entities/
│   │   │   ├── Job.cs                 # Job entity
│   │   │   └── JobItemLog.cs          # Log entry
│   │   └── Interfaces/
│   │       ├── IJobRepository.cs
│   │       ├── IJobService.cs
│   │       └── IItemProcessor.cs
│   │
│   ├── JobProcessingApi.Application/
│   │   ├── Services/
│   │   │   └── JobService.cs          # Job orchestration
│   │   ├── Strategies/
│   │   │   ├── IJobProcessingStrategy.cs
│   │   │   ├── BulkJobProcessingStrategy.cs
│   │   │   ├── BatchJobProcessingStrategy.cs
│   │   │   └── JobProcessingStrategyFactory.cs
│   │   └── Validators/
│   │       └── StartJobCommandValidator.cs
│   │
│   └── JobProcessingApi.Infrastructure/
│       ├── Repositories/
│       │   └── InMemoryJobRepository.cs
│       └── Services/
│           └── MockItemProcessor.cs   # Simulates external processor
│
└── tests/
    └── JobProcessingApi.Tests/
        ├── Application/
        │   ├── Services/
        │   └── Strategies/
        └── Infrastructure/
            └── Repositories/
```

---

## 🔧 Configuration

### appsettings.json

```json
{
  "Jwt": {
    "Key": "YourSuperSecretKeyThatShouldBeStoredSecurely12345",
    "Issuer": "JobProcessingApi",
    "Audience": "JobProcessingApiClients",
    "ExpirationMinutes": 60
  }
}
```

**Production:** Use environment variables or Azure Key Vault for secrets.

---

## 🚀 Deployment

This solution can be deployed to:

**Azure:**
- App Service
- Container Instances
- AKS (Kubernetes)

**AWS:**
- Elastic Beanstalk
- ECS/Fargate
- EKS (Kubernetes)

**Docker:**
```bash
docker build -t jobprocessing-api .
docker run -p 8080:80 jobprocessing-api
```

---

## 🔄 Extending the System

### Add a New Job Type

1. Create new strategy:
```csharp
public class PriorityJobProcessingStrategy : IJobProcessingStrategy
{
    public JobType JobType => JobType.Priority;
    public async Task ExecuteAsync(...) { /* logic */ }
}
```

2. Register in DI:
```csharp
services.AddScoped<IJobProcessingStrategy, PriorityJobProcessingStrategy>();
```

3. Add enum value:
```csharp
public enum JobType { Bulk = 0, Batch = 1, Priority = 2 }
```

**No changes needed** to existing strategies, factory, or service!

---

## 🐛 Troubleshooting

**Port conflict:**
```bash
# Change port in Properties/launchSettings.json
"applicationUrl": "https://localhost:7001"
```

**Authentication errors:**
```bash
# Verify JWT settings in appsettings.json
# Or disable auth by commenting [Authorize] in JobsController.cs
```

**Tests failing:**
```bash
dotnet clean
dotnet build
dotnet test
```

---

## 📚 Technologies Used

- **.NET 8.0** - Framework
- **ASP.NET Core** - Web API
- **C# 12** - Language
- **Serilog** - Structured logging
- **FluentValidation** - Input validation
- **JWT Bearer** - Authentication
- **Swagger/OpenAPI** - API documentation
- **xUnit** - Testing framework
- **Moq** - Mocking library
- **FluentAssertions** - Test assertions

---

## ✅ Assignment Requirements Met

**MUST-HAVE:**
- ✅ Solution compiles (0 errors)
- ✅ Proper .NET Core usage
- ✅ Clean code and SOLID principles
- ✅ Dependency injection throughout
- ✅ N-layer architecture
- ✅ START JOB feature fully functional
- ✅ Comprehensive unit tests (17/17 passing)

**SHOULD-HAVE:**
- ✅ Design patterns (Strategy, Factory, Repository)
- ✅ Proper logging (Serilog with structured logs)

**NICE-TO-HAVE:**
- ✅ Middleware (exception handling, request logging)
- ✅ API documentation (Swagger/OpenAPI)
- ✅ Authentication (JWT)
- ✅ Deployment guides (Azure, AWS, Docker)

---

## 👤 Author

Senior Backend Developer Candidate  
Assignment for Ohpen Engineering Team

---


**Last Updated:** February 2025  
**Version:** 1.0.0
