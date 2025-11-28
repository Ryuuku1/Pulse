# Multi-stage Dockerfile for ASP.NET Core API

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["src/SolarMonitor.Api/SolarMonitor.Api.csproj", "src/SolarMonitor.Api/"]
COPY ["src/SolarMonitor.Application/SolarMonitor.Application.csproj", "src/SolarMonitor.Application/"]
COPY ["src/SolarMonitor.Domain/SolarMonitor.Domain.csproj", "src/SolarMonitor.Domain/"]
COPY ["src/SolarMonitor.Infrastructure/SolarMonitor.Infrastructure.csproj", "src/SolarMonitor.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/SolarMonitor.Api/SolarMonitor.Api.csproj"

# Copy all source files
COPY . .

# Build and publish
WORKDIR "/src/src/SolarMonitor.Api"
RUN dotnet publish "SolarMonitor.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "SolarMonitor.Api.dll"]
