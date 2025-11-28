# Solar Monitor

A complete, production-ready application for monitoring Huawei FusionSolar / SUN2000 solar inverters and plants using Clean Architecture principles.

## 📋 Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Running Locally](#running-locally)
- [Running with Docker](#running-with-docker)
- [API Documentation](#api-documentation)
- [Project Structure](#project-structure)
- [Clean Architecture Explained](#clean-architecture-explained)
- [Huawei API Integration](#huawei-api-integration)
- [Future Enhancements](#future-enhancements)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)
- [License](#license)

## 🎯 Overview

Solar Monitor provides real-time and historical dashboards for Huawei FusionSolar solar plants. The application:

- **Monitors** solar production, grid power, energy summaries in real-time
- **Displays** plant and device status, metrics, and historical trends
- **Polls** the Huawei FusionSolar API automatically in the background
- **Caches** data locally to minimize API calls and improve performance
- **Exposes** a clean REST API for frontend consumption
- **Designed** with Clean Architecture for maintainability and testability
- **Ready** for deployment behind a reverse proxy with HTTPS

## 🏗 Architecture

This project follows **Clean Architecture** (also known as Hexagonal Architecture or Ports and Adapters) to ensure:

- **Separation of Concerns**: Each layer has a single, well-defined responsibility
- **Testability**: Core business logic is independent of frameworks and infrastructure
- **Maintainability**: Changes in one layer don't ripple through the entire codebase
- **Future-Proofing**: Easy to swap out infrastructure (e.g., change from Huawei API to another vendor)

### Architecture Diagram

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                    │
│  ┌──────────────┐           ┌──────────────────────┐   │
│  │   Frontend   │           │   ASP.NET Core API   │   │
│  │  React + TS  │◄─────────►│    (Controllers)     │   │
│  └──────────────┘           └──────────────────────┘   │
└──────────────────────────────────┬──────────────────────┘
                                   │
┌──────────────────────────────────▼──────────────────────┐
│                   Application Layer                      │
│  ┌────────────────────────────────────────────────┐    │
│  │  Use Cases / Services (IPlantService, etc.)    │    │
│  │  Ports (IHuaweiClient, IMetricsCache)          │    │
│  └────────────────────────────────────────────────┘    │
└──────────────────────────────────┬──────────────────────┘
                                   │
┌──────────────────────────────────▼──────────────────────┐
│                   Domain Layer (Core)                    │
│  ┌────────────────────────────────────────────────┐    │
│  │  Entities: Plant, Device                       │    │
│  │  Value Objects: RealtimeMetrics, EnergySummary │    │
│  │  Domain Logic & Rules                          │    │
│  └────────────────────────────────────────────────┘    │
└──────────────────────────────────┬──────────────────────┘
                                   │
┌──────────────────────────────────▼──────────────────────┐
│                 Infrastructure Layer                     │
│  ┌────────────────────────────────────────────────┐    │
│  │  HuaweiClient (implements IHuaweiClient)       │    │
│  │  InMemoryMetricsCache (implements IMetricsCache)│   │
│  │  HuaweiDataPollingService (Background Service) │    │
│  └────────────────────────────────────────────────┘    │
└──────────────────────────────────┬──────────────────────┘
                                   │
                              ┌────▼────┐
                              │ Huawei  │
                              │   API   │
                              └─────────┘
```

### Dependency Flow

```
API Layer        →  Application Layer  →  Domain Layer
                 ↓
Infrastructure Layer
```

**Key Rule**: Dependencies point **inward**. The Domain layer has NO dependencies on other layers. The Application layer depends only on the Domain. Infrastructure implements interfaces defined in Application.

## ✨ Features

### Current Features

- ✅ **Real-time monitoring**: View current power, voltage, grid status
- ✅ **Energy summaries**: Daily, monthly, yearly, and lifetime energy production
- ✅ **Plant management**: List plants and devices
- ✅ **Background polling**: Automatic data sync from Huawei API
- ✅ **REST API**: Clean, documented API endpoints
- ✅ **Docker support**: Full containerization for easy deployment
- ✅ **Health checks**: Built-in health endpoints for monitoring
- ✅ **CORS configured**: Frontend can call API from different origins
- ✅ **Swagger/OpenAPI**: Interactive API documentation at `/swagger`

### Planned Features

- ⏳ **SignalR real-time updates**: Push updates to frontend when new data arrives
- ⏳ **Historical charts**: Visualize power/energy over time
- ⏳ **Alarms and events**: Display plant/device alerts
- ⏳ **SQLite persistence**: Store historical data for longer retention
- ⏳ **Authentication**: Secure API with JWT tokens
- ⏳ **Multi-user support**: User accounts and plant access control

## 🛠 Technology Stack

### Backend

- **.NET 10.0** (latest LTS at time of creation)
- **ASP.NET Core** for REST API
- **Clean Architecture** with 4 layers (Domain, Application, Infrastructure, API)
- **Swagger/Swashbuckle** for API documentation
- **HttpClient** with Polly (future: retries and resilience)
- **Background Services** for polling Huawei API

### Frontend

- **React 18** with **TypeScript**
- **Vite** for build tooling
- **React Query (TanStack Query)** for data fetching
- **React Router** for routing
- **Recharts** for charting
- **Date-fns** for date manipulation

### DevOps

- **Docker** & **Docker Compose** for containerization
- **Nginx** for serving the React frontend
- **.env** for environment-based configuration

## 📦 Prerequisites

### For Local Development (without Docker)

- **.NET 10.0 SDK** or later ([Download](https://dotnet.microsoft.com/download))
- **Node.js 20+** and **npm** ([Download](https://nodejs.org/))
- **Huawei FusionSolar OpenAPI account** (contact `eu_inverter_support@huawei.com`)

### For Docker Deployment

- **Docker** 20.10+ ([Download](https://www.docker.com/get-started))
- **Docker Compose** 2.0+ (usually included with Docker Desktop)
- **Huawei FusionSolar OpenAPI account**

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone <your-repo-url>
cd solar-monitor
```

### 2. Configure Huawei API Credentials

Copy the example environment file and fill in your credentials:

```bash
cp .env.example .env
```

Edit `.env` and set:

```env
HUAWEI_BASE_URL=https://eu5.fusionsolar.huawei.com
HUAWEI_USERNAME=your-username
HUAWEI_PASSWORD=your-password
```

**Important**: The Huawei FusionSolar API has **very strict rate limits**:
- Approximately **1 request per minute** per endpoint
- **Single login session** limit (logging in elsewhere invalidates your token)

To obtain an OpenAPI account:
1. Log into the FusionSolar web portal as a company administrator
2. Navigate to **System > Company Management > Northbound Management**
3. Or contact `eu_inverter_support@huawei.com` directly

### 3. Run with Docker Compose (Recommended)

The easiest way to run the full stack:

```bash
docker-compose up --build
```

This will:
- Build the backend ASP.NET Core API
- Build the frontend React app
- Start both services
- Backend available at `http://localhost:5000`
- Frontend available at `http://localhost:8080`
- Swagger UI at `http://localhost:5000/swagger`

To run in detached mode:

```bash
docker-compose up -d
```

To stop:

```bash
docker-compose down
```

### 4. Run Locally (for Development)

#### Backend

```bash
cd backend
dotnet restore
dotnet build

# Update appsettings.json with your Huawei credentials
# Then run:
cd src/SolarMonitor.Api
dotnet run
```

Backend will be available at `http://localhost:5000` (or `https://localhost:5001`)

#### Frontend

```bash
cd frontend
npm install

# Create .env.local with API URL
echo "VITE_API_BASE_URL=http://localhost:5000" > .env.local

npm run dev
```

Frontend will be available at `http://localhost:5173`

## ⚙ Configuration

### Backend Configuration (appsettings.json)

Located at `backend/src/SolarMonitor.Api/appsettings.json`:

```json
{
  "Huawei": {
    "BaseUrl": "https://eu5.fusionsolar.huawei.com",
    "Username": "your-username",
    "Password": "your-password",
    "StationCode": "optional",
    "PollingIntervalSeconds": 30,
    "RequestTimeoutSeconds": 30
  }
}
```

**Note**: In production, use **environment variables** instead of hardcoding credentials:

```bash
export Huawei__Username="your-username"
export Huawei__Password="your-password"
```

Or in Docker via `.env` file (see `.env.example`).

### Frontend Configuration

Create `frontend/.env.local`:

```env
VITE_API_BASE_URL=http://localhost:5000
```

## 📂 Project Structure

```
solar-monitor/
├── backend/
│   ├── src/
│   │   ├── SolarMonitor.Domain/          # Core domain entities & logic
│   │   │   ├── Entities/
│   │   │   │   ├── Plant.cs
│   │   │   │   └── Device.cs
│   │   │   ├── ValueObjects/
│   │   │   │   ├── RealtimeMetrics.cs
│   │   │   │   ├── EnergySummary.cs
│   │   │   │   └── TimeseriesPoint.cs
│   │   │   └── Common/
│   │   │       └── Result.cs
│   │   ├── SolarMonitor.Application/     # Use cases, ports, DTOs
│   │   │   ├── Ports/
│   │   │   │   ├── IHuaweiClient.cs
│   │   │   │   └── IMetricsCache.cs
│   │   │   ├── Services/
│   │   │   │   ├── IPlantService.cs
│   │   │   │   ├── PlantService.cs
│   │   │   │   ├── IMetricsService.cs
│   │   │   │   └── MetricsService.cs
│   │   │   └── DependencyInjection.cs
│   │   ├── SolarMonitor.Infrastructure/  # External integrations
│   │   │   ├── Huawei/
│   │   │   │   ├── HuaweiClient.cs
│   │   │   │   └── DTOs/
│   │   │   ├── Caching/
│   │   │   │   └── InMemoryMetricsCache.cs
│   │   │   ├── BackgroundServices/
│   │   │   │   └── HuaweiDataPollingService.cs
│   │   │   ├── Configuration/
│   │   │   │   └── HuaweiApiOptions.cs
│   │   │   └── DependencyInjection.cs
│   │   └── SolarMonitor.Api/             # REST API, controllers
│   │       ├── Controllers/
│   │       │   ├── PlantsController.cs
│   │       │   └── MetricsController.cs
│   │       ├── Models/
│   │       │   └── ApiResponse.cs
│   │       ├── Program.cs
│   │       ├── appsettings.json
│   │       └── Dockerfile
│   ├── tests/
│   │   ├── SolarMonitor.Domain.Tests/
│   │   ├── SolarMonitor.Application.Tests/
│   │   ├── SolarMonitor.Infrastructure.Tests/
│   │   └── SolarMonitor.Api.Tests/
│   ├── SolarMonitor.sln
│   └── nuget.config
├── frontend/
│   ├── src/
│   │   ├── api/                          # API client
│   │   ├── features/                     # Feature modules
│   │   ├── components/                   # Shared components
│   │   ├── types/                        # TypeScript types
│   │   └── App.tsx
│   ├── Dockerfile
│   ├── nginx.conf
│   ├── package.json
│   └── vite.config.ts
├── docker-compose.yml
├── .env.example
└── README.md
```

## 🏛 Clean Architecture Explained

### Why Clean Architecture?

Clean Architecture ensures that:
1. **Business logic** (Domain layer) is isolated from external concerns
2. **Dependencies flow inward**: Outer layers depend on inner layers, never the reverse
3. **Testability**: You can test business logic without needing a database, API, or UI
4. **Flexibility**: Swap out infrastructure (e.g., switch from Huawei API to another vendor) without changing domain logic

### Layer Responsibilities

#### 1. Domain Layer (`SolarMonitor.Domain`)

**Purpose**: Pure business logic, entities, and value objects

**Contains**:
- **Entities**: `Plant`, `Device` (objects with identity)
- **Value Objects**: `RealtimeMetrics`, `EnergySummary`, `TimeseriesPoint` (immutable data)
- **Domain services**: Logic that doesn't belong to a single entity
- **NO** dependencies on other projects

**Example**: The `Plant` entity knows its ID, name, capacity, status — but has NO knowledge of HTTP, databases, or Huawei API.

#### 2. Application Layer (`SolarMonitor.Application`)

**Purpose**: Orchestrate use cases and define ports (interfaces) for external services

**Contains**:
- **Services/Use Cases**: `IPlantService`, `IMetricsService`
- **Ports (Interfaces)**: `IHuaweiClient`, `IMetricsCache`
- **DTOs**: Data transfer objects for requests/responses
- **Depends on**: Domain layer ONLY

**Example**: `PlantService` retrieves plant data from `IMetricsCache` (a port). It doesn't care HOW the cache is implemented (in-memory, Redis, SQL) — that's the Infrastructure's job.

#### 3. Infrastructure Layer (`SolarMonitor.Infrastructure`)

**Purpose**: Implement the ports defined in Application layer

**Contains**:
- **HuaweiClient**: Implements `IHuaweiClient` using HttpClient to call Huawei API
- **InMemoryMetricsCache**: Implements `IMetricsCache` using in-memory storage
- **HuaweiDataPollingService**: Background service that polls Huawei API periodically
- **Configuration**: `HuaweiApiOptions` for settings
- **Depends on**: Application AND Domain layers

**Example**: `HuaweiClient` makes HTTP calls, parses JSON, handles auth, and maps responses to Domain models.

#### 4. API Layer (`SolarMonitor.Api`)

**Purpose**: Expose REST endpoints and configure the application

**Contains**:
- **Controllers**: `PlantsController`, `MetricsController`
- **Program.cs**: Application startup, DI configuration
- **Middleware**: Error handling, logging
- **Swagger**: API documentation
- **Depends on**: Application (and Domain if needed)

**Example**: `PlantsController` calls `IPlantService` (from Application layer) to get data, then returns it as JSON.

### Dependency Injection (DI)

The Api layer wires everything together in `Program.cs`:

```csharp
builder.Services.AddApplicationServices();  // Register Application services
builder.Services.AddInfrastructureServices(builder.Configuration);  // Register Infrastructure
```

This is where **Dependency Inversion** happens: The API doesn't create concrete instances itself; it asks the DI container to inject them.

## 🔌 Huawei API Integration

### Endpoints Used

The application uses the following Huawei FusionSolar Northbound API endpoints:

1. **Authentication**
   - **POST** `/thirdData/login`
   - Body: `{ "userName": "...", "systemCode": "..." }`
   - Returns: `{ "success": true, "data": { "token": "..." } }`

2. **Get Plant List**
   - **POST** `/thirdData/getStationList`
   - Headers: `XSRF-TOKEN: <token>`
   - Returns: List of plants/stations

3. **Get Devices for a Plant**
   - **POST** `/thirdData/getDevList`
   - Body: `{ "stationCodes": "plantId" }`
   - Returns: List of devices (inverters, batteries, etc.)

4. **Get Real-time KPIs**
   - **POST** `/thirdData/getStationRealKpi`
   - Body: `{ "stationCodes": "plantId" }`
   - Returns: Current power, energy today, etc.

5. **Get Daily Energy Summary**
   - **POST** `/thirdData/getKpiStationDay`
   - Body: `{ "stationCodes": "plantId" }`
   - Returns: Energy today, month, year, total

6. **Get Device Real-time KPIs** (future)
   - **POST** `/thirdData/getDevRealKpi`

7. **Get Historical Data** (future)
   - **POST** `/thirdData/getKpiStationHour`
   - Body: `{ "stationCodes": "plantId", "collectTime": timestamp }`

### Rate Limits & Constraints

⚠ **Important**: The Huawei FusionSolar API has **very strict rate limits**:

- **~1 request per minute** per endpoint
- **Single concurrent session**: Logging in elsewhere invalidates your token
- **Token expiry**: Tokens expire after ~1 hour (implementation auto-refreshes)

**Mitigation Strategies**:
1. **Background polling**: The `HuaweiDataPollingService` polls at a configurable interval (default 30 seconds)
2. **Caching**: All data is cached in `IMetricsCache` so the frontend reads from cache, not the API directly
3. **Avoid parallel requests**: The implementation serializes API calls to avoid hitting rate limits

### Field Mappings (Assumptions)

The Huawei API returns data in a `dataItemMap` dictionary. Field names may vary by region/version. This implementation assumes:

- `total_power` → `PvPowerKw`
- `day_power` → `EnergyTodayKwh`
- `month_power` → `EnergyMonthKwh`
- `year_power` → `EnergyYearKwh`

**You may need to adjust** these mappings in `HuaweiClient.cs` based on actual API responses. Check the Swagger docs or API responses from your specific Huawei setup.

### Authentication Flow

1. On startup, `HuaweiDataPollingService` calls `IHuaweiClient.LoginAsync()`
2. `HuaweiClient` stores the token and sets an expiry time (1 hour)
3. Before each API call, `HuaweiClient` checks if the token is expired and refreshes if needed
4. The token is passed in the `XSRF-TOKEN` header for all subsequent requests
5. On shutdown, the service calls `LogoutAsync()` to invalidate the session

## 📡 API Documentation

Once the backend is running, navigate to:

**Swagger UI**: `http://localhost:5000/swagger`

### Key Endpoints

#### GET `/health`

Health check endpoint.

**Response**:
```json
{
  "status": "healthy",
  "timestamp": "2025-11-28T12:00:00Z"
}
```

#### GET `/api/plants`

Get all plants.

**Response**:
```json
{
  "success": true,
  "data": [
    {
      "id": "NE=12345678",
      "name": "My Solar Plant",
      "address": "123 Solar St",
      "installedCapacityKw": 10.5,
      "latitude": 52.52,
      "longitude": 13.405,
      "status": 1,
      "lastUpdateTime": "2025-11-28T12:00:00Z"
    }
  ]
}
```

#### GET `/api/plants/{plantId}`

Get plant summary with current metrics and energy summary.

**Response**:
```json
{
  "success": true,
  "data": {
    "plant": { ... },
    "currentMetrics": {
      "timestampUtc": "2025-11-28T12:00:00Z",
      "pvPowerKw": 8.5,
      "gridPowerKw": 7.2,
      "dayEnergyKwh": 42.5
    },
    "energySummary": {
      "energyTodayKwh": 42.5,
      "energyMonthKwh": 850.0,
      "energyYearKwh": 9500.0,
      "energyTotalKwh": 25000.0
    },
    "totalDevices": 1,
    "activeDevices": 1
  }
}
```

#### GET `/api/plants/{plantId}/devices`

Get devices for a plant.

#### GET `/api/metrics/plants/{plantId}/realtime`

Get real-time metrics for a plant.

#### GET `/api/metrics/plants/{plantId}/timeseries?metricType=Power&from=2025-11-28T00:00:00Z&to=2025-11-28T23:59:59Z`

Get historical timeseries data (future).

## 🔐 Future: Exposing to the Internet

This application is currently designed to run on your local network. To expose it securely to the internet:

### Option 1: Reverse Proxy with HTTPS

1. **Set up a reverse proxy** (Nginx or Caddy) on a VPS or your local machine
2. **Configure TLS/SSL** with Let's Encrypt
3. **Forward requests** to your Docker containers

Example Nginx config:

```nginx
server {
    listen 443 ssl http2;
    server_name solar.yourdomain.com;

    ssl_certificate /etc/letsencrypt/live/solar.yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/solar.yourdomain.com/privkey.pem;

    # Frontend
    location / {
        proxy_pass http://localhost:8080;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # Backend API
    location /api/ {
        proxy_pass http://localhost:5000/api/;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

### Option 2: Traefik with Docker

Use Traefik as a reverse proxy with automatic HTTPS:

```yaml
# Add to docker-compose.yml
services:
  traefik:
    image: traefik:v2.10
    command:
      - "--providers.docker=true"
      - "--entrypoints.websecure.address=:443"
      - "--certificatesresolvers.myresolver.acme.tlschallenge=true"
      - "--certificatesresolvers.myresolver.acme.email=you@example.com"
      - "--certificatesresolvers.myresolver.acme.storage=/letsencrypt/acme.json"
    ports:
      - "443:443"
    volumes:
      - "/var/run/docker.sock:/var/run/docker.sock:ro"
      - "./letsencrypt:/letsencrypt"
```

**Security Recommendations**:
- Use **HTTPS only** (redirect HTTP to HTTPS)
- Add **authentication** (JWT, OAuth, or simple API key)
- Configure **rate limiting**
- Use **firewall rules** to restrict access if needed

## 🐛 Troubleshooting

### Backend Issues

**Error: "Authentication failed"**

- Check your Huawei credentials in `.env` or `appsettings.json`
- Verify your OpenAPI account is active
- Ensure you're using the correct base URL for your region

**Error: "No plants available. Data may not have been synced yet."**

- Wait ~30 seconds for the first polling cycle to complete
- Check backend logs: `docker logs solarmonitor-backend`
- Verify your Huawei account has access to plants

**Error: Rate limit / API errors**

- The Huawei API has strict rate limits
- Increase `PollingIntervalSeconds` in configuration
- Check Huawei API status

### Frontend Issues

**Cannot connect to backend**

- Verify backend is running: `curl http://localhost:5000/health`
- Check CORS configuration in `Program.cs`
- Ensure `VITE_API_BASE_URL` is set correctly

### Docker Issues

**Port already in use**

- Change ports in `docker-compose.yml`:
  ```yaml
  ports:
    - "8001:8080"  # Change 8001 to any free port
  ```

**Build fails**

- Run `docker-compose build --no-cache` to rebuild from scratch

## 🤝 Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- [Huawei FusionSolar](https://www.huawei.com/en/power-energy-digital/fusionsolar) for providing the API
- Clean Architecture principles by [Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- The .NET and React communities

## 📞 Support

For Huawei API support: `eu_inverter_support@huawei.com`

For application issues: [Open an issue](https://github.com/yourusername/solar-monitor/issues)

---

**Built with ❤️ using Clean Architecture, .NET 10, and React**

## Sources

Research for Huawei FusionSolar API integration:
- [HUAWEI FusionSolar API - Meteocontrol](https://help-center.meteocontrol.com/en/vcom-cloud/latest/huawei-fusionsolar-api-1)
- [GitHub - tijsverkoyen/HomeAssistant-FusionSolar](https://github.com/tijsverkoyen/HomeAssistant-FusionSolar)
- [SmartPVMS 24.7.0 Northbound API Reference - Huawei](https://support.huawei.com/enterprise/en/doc/EDOC1100427895)
- [Set up API access - Huawei FusionSolar API](https://kb.solytic.com/en/knowledge/set-up-api-key-huawei-fusionsolar)
