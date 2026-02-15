# Visual Studio 2022 Setup Guide

## 🔧 Troubleshooting "Project Not Found" Issues

If you see "unavailable" or "not found" errors when opening the solution in Visual Studio, follow these steps:

### Option 1: Quick Fix - Let Visual Studio Recreate the Solution

1. **Don't open the .sln file directly**
2. **Instead, create a new solution in Visual Studio:**

   ```
   1. Open Visual Studio 2022
   2. File → New → Project
   3. Select "Blank Solution"
   4. Name: JobProcessingApi
   5. Location: Choose where you extracted the files
   6. Click Create
   ```

3. **Add existing projects to the solution:**

   ```
   Right-click on Solution 'JobProcessingApi' in Solution Explorer
   → Add → Existing Project
   
   Add these projects IN THIS ORDER:
   
   1. src\JobProcessingApi.Core\JobProcessingApi.Core.csproj
   2. src\JobProcessingApi.Infrastructure\JobProcessingApi.Infrastructure.csproj
   3. src\JobProcessingApi.Application\JobProcessingApi.Application.csproj
   4. src\JobProcessingApi.API\JobProcessingApi.API.csproj
   5. tests\JobProcessingApi.Tests\JobProcessingApi.Tests.csproj
   ```

4. **Set the startup project:**
   ```
   Right-click JobProcessingApi.API → Set as Startup Project
   ```

5. **Build and run:**
   ```
   Build → Build Solution (Ctrl+Shift+B)
   Debug → Start Without Debugging (Ctrl+F5)
   ```

---

### Option 2: Start Completely Fresh in Visual Studio

This is the **RECOMMENDED** approach if you want the cleanest setup:

#### Step 1: Create the Solution Structure

1. **Open Visual Studio 2022**

2. **Create the API project first:**
   ```
   File → New → Project
   → Search for "ASP.NET Core Web API"
   → Click Next
   
   Project name: JobProcessingApi.API
   Location: C:\Projects\JobProcessingApi\src
   Solution name: JobProcessingApi
   ✓ Place solution and project in the same directory: UNCHECKED
   → Click Next
   
   Framework: .NET 8.0
   Authentication type: None (we'll add JWT manually)
   ✓ Configure for HTTPS: CHECKED
   ✓ Enable OpenAPI support: CHECKED
   ✓ Use controllers: CHECKED
   → Click Create
   ```

3. **Add the Core library:**
   ```
   Right-click Solution → Add → New Project
   → Search for "Class Library"
   → Click Next
   
   Project name: JobProcessingApi.Core
   Location: C:\Projects\JobProcessingApi\src
   → Click Next
   
   Framework: .NET 8.0
   → Click Create
   
   Delete the default Class1.cs file
   ```

4. **Add the Application library:**
   ```
   Right-click Solution → Add → New Project
   → Class Library
   
   Project name: JobProcessingApi.Application
   Location: C:\Projects\JobProcessingApi\src
   Framework: .NET 8.0
   → Click Create
   
   Delete Class1.cs
   ```

5. **Add the Infrastructure library:**
   ```
   Right-click Solution → Add → New Project
   → Class Library
   
   Project name: JobProcessingApi.Infrastructure
   Location: C:\Projects\JobProcessingApi\src
   Framework: .NET 8.0
   → Click Create
   
   Delete Class1.cs
   ```

6. **Add the Test project:**
   ```
   Right-click Solution → Add → New Project
   → Search for "xUnit Test Project"
   → Click Next
   
   Project name: JobProcessingApi.Tests
   Location: C:\Projects\JobProcessingApi\tests
   → Click Next
   
   Framework: .NET 8.0
   → Click Create
   ```

#### Step 2: Set Up Project References

1. **JobProcessingApi.Application references:**
   ```
   Right-click JobProcessingApi.Application → Add → Project Reference
   ✓ Check: JobProcessingApi.Core
   → Click OK
   ```

2. **JobProcessingApi.Infrastructure references:**
   ```
   Right-click JobProcessingApi.Infrastructure → Add → Project Reference
   ✓ Check: JobProcessingApi.Core
   → Click OK
   ```

3. **JobProcessingApi.API references:**
   ```
   Right-click JobProcessingApi.API → Add → Project Reference
   ✓ Check: JobProcessingApi.Core
   ✓ Check: JobProcessingApi.Application
   ✓ Check: JobProcessingApi.Infrastructure
   → Click OK
   ```

4. **JobProcessingApi.Tests references:**
   ```
   Right-click JobProcessingApi.Tests → Add → Project Reference
   ✓ Check: All four projects (API, Core, Application, Infrastructure)
   → Click OK
   ```

#### Step 3: Add NuGet Packages

**For JobProcessingApi.Application:**
```
Right-click project → Manage NuGet Packages
→ Browse tab

Install:
- FluentValidation (11.9.0)
- FluentValidation.DependencyInjectionExtensions (11.9.0)
- Microsoft.Extensions.Logging.Abstractions (8.0.0)
```

**For JobProcessingApi.API:**
```
Right-click project → Manage NuGet Packages

Install:
- Serilog.AspNetCore (8.0.0)
- Serilog.Sinks.Console (5.0.1)
- Serilog.Sinks.File (5.0.0)
- FluentValidation.AspNetCore (11.3.0)
- Microsoft.AspNetCore.Authentication.JwtBearer (8.0.0)
- Swashbuckle.AspNetCore (already installed)
```

**For JobProcessingApi.Tests:**
```
Right-click project → Manage NuGet Packages

Install:
- Moq (4.20.70)
- FluentAssertions (6.12.0)
- Microsoft.AspNetCore.Mvc.Testing (8.0.0)
- xunit (already installed)
- Microsoft.NET.Test.Sdk (already installed)
```

#### Step 4: Copy the Code

Now copy all the source files from my solution into your newly created projects:

1. **Copy folders and files:**
   ```
   From my solution → To your Visual Studio projects:
   
   Core/Entities/* → Your Core project
   Core/Interfaces/* → Your Core project
   
   Application/Services/* → Your Application project
   Application/Strategies/* → Your Application project
   Application/Validators/* → Your Application project
   
   Infrastructure/Repositories/* → Your Infrastructure project
   Infrastructure/Services/* → Your Infrastructure project
   
   API/Controllers/* → Your API project
   API/Middleware/* → Your API project
   API/Program.cs → Your API project (replace existing)
   API/appsettings.json → Your API project (replace existing)
   API/appsettings.Development.json → Your API project
   
   Tests/* → Your Tests project
   ```

2. **Add folders in Visual Studio:**
   ```
   Right-click project → Add → New Folder
   
   Create these folder structures:
   - Core: Entities, Interfaces
   - Application: Services, Strategies, Validators
   - Infrastructure: Repositories, Services
   - API: Controllers, Middleware
   - Tests: Application/Services, Application/Strategies, Infrastructure/Repositories, Integration
   ```

3. **Add existing files:**
   ```
   Right-click folder → Add → Existing Item
   → Browse to my solution files and add them
   ```

#### Step 5: Build and Run

1. **Build Solution:**
   ```
   Build → Build Solution (Ctrl+Shift+B)
   
   Should complete with 0 errors
   ```

2. **Run Tests:**
   ```
   Test → Run All Tests (Ctrl+R, A)
   
   All tests should pass
   ```

3. **Run the API:**
   ```
   Make sure JobProcessingApi.API is the startup project
   Debug → Start Without Debugging (Ctrl+F5)
   
   Browser should open with Swagger UI
   ```

---

## 🚀 Quickest Solution (If Above Seems Too Much)

### Use Command Line to Create Solution

Open Command Prompt or PowerShell in your project folder:

```bash
# Navigate to where you want the project
cd C:\Projects

# Create solution
dotnet new sln -n JobProcessingApi
cd JobProcessingApi

# Create projects
dotnet new webapi -n JobProcessingApi.API -o src/JobProcessingApi.API
dotnet new classlib -n JobProcessingApi.Core -o src/JobProcessingApi.Core
dotnet new classlib -n JobProcessingApi.Application -o src/JobProcessingApi.Application
dotnet new classlib -n JobProcessingApi.Infrastructure -o src/JobProcessingApi.Infrastructure
dotnet new xunit -n JobProcessingApi.Tests -o tests/JobProcessingApi.Tests

# Add projects to solution
dotnet sln add src/JobProcessingApi.API/JobProcessingApi.API.csproj
dotnet sln add src/JobProcessingApi.Core/JobProcessingApi.Core.csproj
dotnet sln add src/JobProcessingApi.Application/JobProcessingApi.Application.csproj
dotnet sln add src/JobProcessingApi.Infrastructure/JobProcessingApi.Infrastructure.csproj
dotnet sln add tests/JobProcessingApi.Tests/JobProcessingApi.Tests.csproj

# Add project references
cd src/JobProcessingApi.Application
dotnet add reference ../JobProcessingApi.Core/JobProcessingApi.Core.csproj

cd ../JobProcessingApi.Infrastructure
dotnet add reference ../JobProcessingApi.Core/JobProcessingApi.Core.csproj

cd ../JobProcessingApi.API
dotnet add reference ../JobProcessingApi.Core/JobProcessingApi.Core.csproj
dotnet add reference ../JobProcessingApi.Application/JobProcessingApi.Application.csproj
dotnet add reference ../JobProcessingApi.Infrastructure/JobProcessingApi.Infrastructure.csproj

cd ../../tests/JobProcessingApi.Tests
dotnet add reference ../../src/JobProcessingApi.API/JobProcessingApi.API.csproj
dotnet add reference ../../src/JobProcessingApi.Core/JobProcessingApi.Core.csproj
dotnet add reference ../../src/JobProcessingApi.Application/JobProcessingApi.Application.csproj
dotnet add reference ../../src/JobProcessingApi.Infrastructure/JobProcessingApi.Infrastructure.csproj

# Go back to solution root
cd ../..

# Add NuGet packages
cd src/JobProcessingApi.Application
dotnet add package FluentValidation
dotnet add package FluentValidation.DependencyInjectionExtensions
dotnet add package Microsoft.Extensions.Logging.Abstractions

cd ../JobProcessingApi.API
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
dotnet add package FluentValidation.AspNetCore
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer

cd ../../tests/JobProcessingApi.Tests
dotnet add package Moq
dotnet add package FluentAssertions
dotnet add package Microsoft.AspNetCore.Mvc.Testing

cd ../..

# Now open in Visual Studio
start JobProcessingApi.sln
```

Then copy all my source code files into the respective projects.

---

## 📝 Final Checklist

After setup, verify:
- [ ] Solution loads without errors
- [ ] All 5 projects visible in Solution Explorer
- [ ] Solution builds successfully (Ctrl+Shift+B)
- [ ] All tests pass (Ctrl+R, A)
- [ ] API runs and opens Swagger UI (Ctrl+F5)
- [ ] No red squiggles in code files

---

## 🆘 Still Having Issues?

### Check .NET Version
```bash
dotnet --version
```
Should be 8.0.x or higher. If not, install from: https://dotnet.microsoft.com/download

### Check Visual Studio Version
```
Help → About Microsoft Visual Studio
```
Should be Visual Studio 2022 version 17.x

### Check Visual Studio Workloads
```
Tools → Get Tools and Features
```
Make sure these are installed:
- ✓ ASP.NET and web development
- ✓ .NET desktop development

---

## 💡 Pro Tip

Once you have it working in Visual Studio, save it and **never touch the .sln file manually again**. Let Visual Studio manage it. Any changes should be done through:
- Add Project: Right-click solution → Add → New/Existing Project
- Add Reference: Right-click project → Add → Project Reference
- Add Package: Right-click project → Manage NuGet Packages

Good luck! 🚀
