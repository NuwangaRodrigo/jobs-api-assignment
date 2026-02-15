# Deployment Guide

This guide provides step-by-step instructions for deploying the Job Processing API to various environments.

## Table of Contents
1. [Local Development](#local-development)
2. [Docker Deployment](#docker-deployment)
3. [Azure Deployment](#azure-deployment)
4. [AWS Deployment](#aws-deployment)
5. [Production Considerations](#production-considerations)

---

## Local Development

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022, VS Code, or Rider

### Steps

1. **Clone/Extract the repository**
   ```bash
   cd JobProcessingApi
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Run tests**
   ```bash
   dotnet test
   ```

5. **Run the API**
   ```bash
   cd src/JobProcessingApi.API
   dotnet run
   ```

6. **Access the API**
   - Swagger UI: `https://localhost:7001`
   - API Base URL: `https://localhost:7001/api`

### Getting a Test Token

Option 1: Use the Auth endpoint
```bash
curl -X POST https://localhost:7001/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username": "test-user"}'
```

Option 2: Temporarily disable authentication
- Comment out `[Authorize]` in `JobsController.cs`

---

## Docker Deployment

### Using Docker Compose (Recommended)

1. **Build and run**
   ```bash
   docker-compose up --build
   ```

2. **Access the API**
   - API: `http://localhost:8080`

3. **Stop the containers**
   ```bash
   docker-compose down
   ```

### Using Docker Only

1. **Build the image**
   ```bash
   docker build -t jobprocessing-api:latest .
   ```

2. **Run the container**
   ```bash
   docker run -d \
     -p 8080:80 \
     -e ASPNETCORE_ENVIRONMENT=Production \
     -e Jwt__Key=YourProductionSecretKey \
     --name jobprocessing-api \
     jobprocessing-api:latest
   ```

3. **View logs**
   ```bash
   docker logs -f jobprocessing-api
   ```

4. **Stop the container**
   ```bash
   docker stop jobprocessing-api
   docker rm jobprocessing-api
   ```

---

## Azure Deployment

### Option 1: Azure App Service

#### Using Azure CLI

1. **Login to Azure**
   ```bash
   az login
   ```

2. **Create Resource Group**
   ```bash
   az group create \
     --name job-processing-rg \
     --location westeurope
   ```

3. **Create App Service Plan**
   ```bash
   az appservice plan create \
     --name job-processing-plan \
     --resource-group job-processing-rg \
     --sku B1 \
     --is-linux
   ```

4. **Create Web App**
   ```bash
   az webapp create \
     --name job-processing-api \
     --resource-group job-processing-rg \
     --plan job-processing-plan \
     --runtime "DOTNET|8.0"
   ```

5. **Configure Application Settings**
   ```bash
   az webapp config appsettings set \
     --name job-processing-api \
     --resource-group job-processing-rg \
     --settings \
       Jwt__Key="YourProductionSecretKey" \
       Jwt__Issuer="JobProcessingApi" \
       Jwt__Audience="JobProcessingApiClients"
   ```

6. **Deploy the Application**
   ```bash
   # Publish the app
   cd src/JobProcessingApi.API
   dotnet publish -c Release -o ./publish
   
   # Create zip file
   cd publish
   zip -r ../publish.zip .
   
   # Deploy to Azure
   az webapp deployment source config-zip \
     --name job-processing-api \
     --resource-group job-processing-rg \
     --src ../publish.zip
   ```

7. **Access the API**
   ```
   https://job-processing-api.azurewebsites.net
   ```

#### Using Azure Portal

1. Go to Azure Portal
2. Create a new Web App
3. Configure:
   - Runtime: .NET 8
   - Operating System: Linux
   - Region: Your preferred region
4. Go to Configuration → Application Settings
5. Add:
   - `Jwt__Key`: Your secret key
   - `Jwt__Issuer`: JobProcessingApi
   - `Jwt__Audience`: JobProcessingApiClients
6. Deploy using:
   - Visual Studio Publish
   - GitHub Actions
   - Azure DevOps

### Option 2: Azure Container Instances

1. **Build and push Docker image**
   ```bash
   # Login to Azure Container Registry
   az acr login --name yourregistry
   
   # Build and push
   docker build -t yourregistry.azurecr.io/jobprocessing-api:latest .
   docker push yourregistry.azurecr.io/jobprocessing-api:latest
   ```

2. **Create Container Instance**
   ```bash
   az container create \
     --resource-group job-processing-rg \
     --name jobprocessing-api \
     --image yourregistry.azurecr.io/jobprocessing-api:latest \
     --dns-name-label jobprocessing-api \
     --ports 80 443 \
     --environment-variables \
       ASPNETCORE_ENVIRONMENT=Production \
       Jwt__Key=YourProductionSecretKey
   ```

---

## AWS Deployment

### Option 1: AWS Elastic Beanstalk

1. **Install EB CLI**
   ```bash
   pip install awsebcli
   ```

2. **Initialize Elastic Beanstalk**
   ```bash
   eb init -p "64bit Amazon Linux 2023 v3.0.0 running .NET 8" job-processing-api
   ```

3. **Create Environment**
   ```bash
   eb create job-processing-api-env
   ```

4. **Set Environment Variables**
   ```bash
   eb setenv \
     ASPNETCORE_ENVIRONMENT=Production \
     Jwt__Key=YourProductionSecretKey \
     Jwt__Issuer=JobProcessingApi \
     Jwt__Audience=JobProcessingApiClients
   ```

5. **Deploy**
   ```bash
   dotnet publish -c Release -o ./publish
   cd publish
   eb deploy
   ```

6. **Open Application**
   ```bash
   eb open
   ```

### Option 2: AWS ECS (Fargate)

1. **Create ECR Repository**
   ```bash
   aws ecr create-repository --repository-name jobprocessing-api
   ```

2. **Build and Push Docker Image**
   ```bash
   # Get ECR login
   aws ecr get-login-password --region us-east-1 | \
     docker login --username AWS --password-stdin \
     123456789012.dkr.ecr.us-east-1.amazonaws.com
   
   # Build and tag
   docker build -t jobprocessing-api .
   docker tag jobprocessing-api:latest \
     123456789012.dkr.ecr.us-east-1.amazonaws.com/jobprocessing-api:latest
   
   # Push
   docker push 123456789012.dkr.ecr.us-east-1.amazonaws.com/jobprocessing-api:latest
   ```

3. **Create ECS Task Definition**
   - Use AWS Console or CLI
   - Configure container with environment variables
   - Set CPU/Memory requirements

4. **Create ECS Service**
   - Choose Fargate launch type
   - Configure networking and load balancer
   - Set desired task count

---

## Production Considerations

### Security

1. **Environment Variables**
   - Never commit secrets to source control
   - Use Azure Key Vault, AWS Secrets Manager, or similar
   
   ```bash
   # Azure Key Vault
   az keyvault secret set \
     --vault-name your-keyvault \
     --name JwtKey \
     --value "YourSecretKey"
   ```

2. **HTTPS**
   - Always use HTTPS in production
   - Configure SSL certificates
   - Enable HSTS

3. **API Keys/Tokens**
   - Implement API key rotation
   - Set appropriate token expiration
   - Use refresh tokens for long-lived sessions

### Monitoring

1. **Application Insights (Azure)**
   ```bash
   # Add to project
   dotnet add package Microsoft.ApplicationInsights.AspNetCore
   
   # Configure in Program.cs
   builder.Services.AddApplicationInsightsTelemetry();
   ```

2. **CloudWatch (AWS)**
   - Configure CloudWatch Logs
   - Set up CloudWatch Alarms
   - Create dashboards

3. **Custom Logging**
   - Already configured with Serilog
   - Logs to file: `logs/jobprocessing-YYYYMMDD.log`
   - Configure log retention policies

### Database Migration

To use a real database instead of in-memory storage:

1. **Add Entity Framework**
   ```bash
   cd src/JobProcessingApi.Infrastructure
   dotnet add package Microsoft.EntityFrameworkCore.SqlServer
   dotnet add package Microsoft.EntityFrameworkCore.Design
   ```

2. **Create DbContext**
   ```csharp
   public class JobProcessingDbContext : DbContext
   {
       public DbSet<Job> Jobs { get; set; }
       public DbSet<JobItemLog> JobItemLogs { get; set; }
   }
   ```

3. **Implement Repository**
   ```csharp
   public class EfJobRepository : IJobRepository
   {
       private readonly JobProcessingDbContext _context;
       // Implement methods using EF Core
   }
   ```

4. **Update DI Registration**
   ```csharp
   builder.Services.AddDbContext<JobProcessingDbContext>(options =>
       options.UseSqlServer(connectionString));
   builder.Services.AddScoped<IJobRepository, EfJobRepository>();
   ```

### Scaling

1. **Horizontal Scaling**
   - Use shared storage (database) instead of in-memory
   - Implement distributed caching (Redis)
   - Use message queues for job processing

2. **Vertical Scaling**
   - Increase container resources
   - Optimize async/await patterns
   - Profile and optimize hot paths

3. **Load Balancing**
   - Azure: App Service automatic load balancing
   - AWS: Application Load Balancer
   - Configure health checks

### Performance Optimization

1. **Caching**
   ```csharp
   builder.Services.AddMemoryCache();
   builder.Services.AddStackExchangeRedisCache(options =>
   {
       options.Configuration = "redis-connection-string";
   });
   ```

2. **Rate Limiting**
   ```csharp
   builder.Services.AddRateLimiter(options =>
   {
       options.AddFixedWindowLimiter("fixed", opt =>
       {
           opt.PermitLimit = 100;
           opt.Window = TimeSpan.FromMinutes(1);
       });
   });
   ```

3. **Response Compression**
   ```csharp
   builder.Services.AddResponseCompression();
   ```

### Backup and Recovery

1. **Database Backups**
   - Configure automated backups
   - Test restore procedures
   - Document recovery time objectives (RTO)

2. **Application State**
   - Persist job state to durable storage
   - Implement checkpointing for long-running jobs
   - Handle service interruptions gracefully

### CI/CD Pipeline

Example GitHub Actions workflow:

```yaml
name: Deploy to Azure

on:
  push:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: 8.0.x
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --configuration Release
    
    - name: Test
      run: dotnet test --no-build --verbosity normal
    
    - name: Publish
      run: dotnet publish -c Release -o ./publish
    
    - name: Deploy to Azure
      uses: azure/webapps-deploy@v2
      with:
        app-name: job-processing-api
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ./publish
```

---

## Health Checks

Add health check endpoint for monitoring:

```csharp
// Program.cs
builder.Services.AddHealthChecks();

// After app.UseAuthorization();
app.MapHealthChecks("/health");
```

---

## Troubleshooting

### Common Issues

1. **Port conflicts**
   - Change ports in `docker-compose.yml` or launch settings

2. **JWT authentication failures**
   - Verify JWT settings match across environments
   - Check token expiration

3. **Database connection issues**
   - Verify connection strings
   - Check firewall rules
   - Ensure database is accessible from deployment environment

4. **Missing logs**
   - Ensure write permissions to logs directory
   - Check Serilog configuration

---

## Support

For issues or questions about deployment:
- Review the main README.md
- Check application logs
- Verify environment variables
- Test locally before deploying

---

**Last Updated**: February 2025  
**Version**: 1.0.0
