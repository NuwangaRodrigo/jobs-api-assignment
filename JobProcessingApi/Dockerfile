# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY JobProcessingApi.sln .
COPY src/JobProcessingApi.API/JobProcessingApi.API.csproj src/JobProcessingApi.API/
COPY src/JobProcessingApi.Core/JobProcessingApi.Core.csproj src/JobProcessingApi.Core/
COPY src/JobProcessingApi.Application/JobProcessingApi.Application.csproj src/JobProcessingApi.Application/
COPY src/JobProcessingApi.Infrastructure/JobProcessingApi.Infrastructure.csproj src/JobProcessingApi.Infrastructure/
COPY tests/JobProcessingApi.Tests/JobProcessingApi.Tests.csproj tests/JobProcessingApi.Tests/

# Restore dependencies
RUN dotnet restore

# Copy remaining source code
COPY . .

# Build the application
WORKDIR /src/src/JobProcessingApi.API
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copy published files
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Create logs directory
RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "JobProcessingApi.API.dll"]
