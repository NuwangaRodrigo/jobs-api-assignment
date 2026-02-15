# Quick Start Guide

Get the Job Processing API up and running in 5 minutes!

## ⚡ Express Setup

### 1. Prerequisites Check
```bash
# Verify .NET 8 is installed
dotnet --version
# Should show 8.0.x or higher
```

### 2. Build & Run
```bash
# Navigate to solution directory
cd JobProcessingApi

# Restore, build, and test
dotnet restore
dotnet build
dotnet test

# Run the API
cd src/JobProcessingApi.API
dotnet run
```

### 3. Access Swagger UI
Open your browser to: **https://localhost:7001**

---

## 🔑 Quick Authentication

### Get a Test Token
```bash
curl -X POST https://localhost:7001/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username": "test-user"}'
```

Copy the token from the response.

---

## 🚀 Test the API

### 1. Start a BULK Job
```bash
curl -X POST https://localhost:7001/api/jobs \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
    "jobType": 0,
    "items": ["item1", "item2", "item3", "item4", "item5"]
  }'
```

**Response:** You'll get a `jobId`
```json
{
  "jobId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "message": "Job created and processing started"
}
```

### 2. Check Job Status
```bash
curl https://localhost:7001/api/jobs/YOUR_JOB_ID/status \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### 3. Get Job Logs
```bash
curl https://localhost:7001/api/jobs/YOUR_JOB_ID/logs \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

---

## 🎯 Test Different Scenarios

### Scenario 1: All Items Succeed
```json
{
  "jobType": 0,
  "items": ["item1-SUCCESS", "item2-SUCCESS", "item3-SUCCESS"]
}
```
Result: Status = `Completed` (2)

### Scenario 2: BULK with Failures
```json
{
  "jobType": 0,
  "items": ["item1-SUCCESS", "item2-FAIL", "item3-SUCCESS", "item4-FAIL"]
}
```
Result: Status = `PartiallyCompleted` (4), processes all items

### Scenario 3: BATCH Stops on Failure
```json
{
  "jobType": 1,
  "items": ["item1-SUCCESS", "item2-FAIL", "item3-SUCCESS"]
}
```
Result: Status = `Failed` (3), stops at item2

---

## 📊 Job Types & Statuses

### Job Types
- `0` = **BULK** (processes all, continues on failure)
- `1` = **BATCH** (stops on first failure)

### Job Statuses
- `0` = Pending
- `1` = Running
- `2` = Completed
- `3` = Failed
- `4` = PartiallyCompleted

### Item Statuses
- `0` = Success
- `1` = Failure

---

## 🐳 Docker Quick Start

```bash
# Build and run with Docker Compose
docker-compose up --build

# API available at http://localhost:8080
```

---

## 🧪 Run Tests Only

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity detailed

# Run specific test class
dotnet test --filter "FullyQualifiedName~BulkJobProcessingStrategyTests"
```

---

## 📱 Using Postman

1. Import `JobProcessingApi.postman_collection.json`
2. Set variable `baseUrl` = `https://localhost:7001`
3. Get token from "Generate Token" request (optional - if auth is enabled)
4. Set `jwtToken` variable with the token
5. Run "Start Bulk Job" or "Start Batch Job"
6. Use returned `jobId` for status and logs requests

---

## ⚙️ Disable Authentication (Development Only)

If you want to test without JWT tokens:

1. Open `src/JobProcessingApi.API/Controllers/JobsController.cs`
2. Comment out the `[Authorize]` attribute:
```csharp
// [Authorize]  // <-- Comment this line
[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
```
3. Rebuild and run

---

## 🔍 View Logs

Logs are written to:
- **Console**: Real-time output
- **Files**: `logs/jobprocessing-YYYYMMDD.log`

```bash
# Tail the log file (Linux/Mac)
tail -f logs/jobprocessing-$(date +%Y%m%d).log

# View with PowerShell (Windows)
Get-Content logs/jobprocessing-$(Get-Date -Format "yyyyMMdd").log -Wait
```

---

## ⏱️ Processing Times

Each item takes approximately **500ms** to process:
- 5 items ≈ 2.5 seconds
- 10 items ≈ 5 seconds
- 100 items ≈ 50 seconds

Monitor progress with the status endpoint!

---

## 🎉 Success Checklist

- [ ] API starts without errors
- [ ] Swagger UI accessible at https://localhost:7001
- [ ] Can generate JWT token
- [ ] Can create a job and get jobId
- [ ] Can check job status
- [ ] Can retrieve job logs
- [ ] Tests pass successfully

---

## 🆘 Troubleshooting

### "Port already in use"
Change port in `src/JobProcessingApi.API/Properties/launchSettings.json`

### "Unable to get JWT token"
Check `appsettings.json` for JWT configuration

### "401 Unauthorized"
- Get a new token (they expire after 60 minutes)
- Or disable authentication (see above)

### "Tests failing"
```bash
# Clean and rebuild
dotnet clean
dotnet build
dotnet test
```

---

## 📚 Next Steps

- Read the full [README.md](README.md)
- Check [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md) for production deployment
- Explore the Swagger UI for interactive API documentation
- Review the code structure and tests

---

**Enjoy testing the Job Processing API!** 🚀
