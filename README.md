# Devices API

A RESTful API for managing device resources, built with **.NET 10** and **ASP.NET Core** following **Domain-Driven Design (DDD)** principles. The API supports full CRUD operations with domain-specific validations, persists data in **PostgreSQL**, and is fully containerized with **Docker**.

## Table of Contents

- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Getting Started](#getting-started)
- [API Documentation](#api-documentation)
- [API Endpoints](#api-endpoints)
- [Domain Validations](#domain-validations)
- [Running Tests](#running-tests)
- [Project Structure](#project-structure)
- [Known Limitations](#known-limitations)
- [Future Improvements](#future-improvements)

## Architecture

The application follows **Domain-Driven Design (DDD)** with a clean layered architecture and strict dependency rules:

```
┌──────────────────────────────────┐
│         DevicesApi.Api           │  ← Presentation Layer
│  Controllers, Middleware,        │     (depends on Application + Infrastructure)
│  Program.cs, Swagger             │
├──────────────────────────────────┤
│      DevicesApi.Application      │  ← Application Layer
│  Commands, Results, Interfaces,  │     (depends on Domain only)
│  Application Services, Mappers   │
├──────────────────────────────────┤
│     DevicesApi.Infrastructure    │  ← Infrastructure Layer
│  EF Core DbContext, Configs,     │     (depends on Domain only)
│  Repository Implementations,     │
│  Migrations                      │
├──────────────────────────────────┤
│        DevicesApi.Domain         │  ← Domain Layer (Core)
│  Rich Entities, Value Objects,   │     (zero external dependencies)
│  Domain Exceptions, Repository   │
│  Interfaces (Ports)              │
└──────────────────────────────────┘
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://www.docker.com/get-started) and Docker Compose

### Option 1: Run with Docker Compose (Recommended)

This is the easiest way to get the API running with PostgreSQL:

```bash
docker-compose up --build
```

The API will be available at: **http://localhost:8080**

Swagger UI: **http://localhost:8080**

### Option 2: Run Locally

1. **Start PostgreSQL** (e.g., via Docker):
   ```bash
   docker run -d --name postgres-devices \
     -e POSTGRES_DB=devicesdb \
     -e POSTGRES_USER=postgres \
     -e POSTGRES_PASSWORD=postgres \
     -p 5432:5432 \
     postgres:17-alpine
   ```

2. **Run the application**:
   ```bash
   cd src/DevicesApi.Api
   dotnet run
   ```

   The API will be available at the port shown in console output.

### Database Migrations

Migrations are applied automatically on application startup. To manually manage migrations:

```bash
# Add a new migration
dotnet ef migrations add <MigrationName> \
  --project src/DevicesApi.Infrastructure \
  --startup-project src/DevicesApi.Api

# Apply migrations
dotnet ef database update \
  --project src/DevicesApi.Infrastructure \
  --startup-project src/DevicesApi.Api
```

## API Documentation

Interactive API documentation is available via **Swagger UI** at the root URL when the application is running.

- **Docker**: http://localhost:8080
- **Local**: https://localhost:{port}

The OpenAPI specification is available at `/swagger/v1/swagger.json`.

## API Endpoints

### Device Resource

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/devices` | Create a new device |
| `GET` | `/api/devices` | Fetch all devices |
| `GET` | `/api/devices?brand={brand}` | Fetch devices by brand |
| `GET` | `/api/devices?state={state}` | Fetch devices by state |
| `GET` | `/api/devices/{id}` | Fetch a single device |
| `PATCH` | `/api/devices/{id}` | Partially update a device |
| `PUT` | `/api/devices/{id}` | Fully update a device |
| `DELETE` | `/api/devices/{id}` | Delete a device |

### Device States

- `Available` — The device is available for use
- `InUse` — The device is currently in use
- `Inactive` — The device is inactive

### Example Requests

**Create a device:**
```bash
curl -X POST http://localhost:8080/api/devices \
  -H "Content-Type: application/json" \
  -d '{"name": "iPhone 15", "brand": "Apple", "state": "Available"}'
```

**Partially update a device:**
```bash
curl -X PATCH http://localhost:8080/api/devices/{id} \
  -H "Content-Type: application/json" \
  -d '{"name": "iPhone 15 Pro"}'
```

**Filter by brand:**
```bash
curl http://localhost:8080/api/devices?brand=Apple
```

**Filter by state:**
```bash
curl http://localhost:8080/api/devices?state=Available
```

### Response Format

**Success (Device):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "iPhone 15",
  "brand": "Apple",
  "state": "Available",
  "creationTime": "2026-02-17T18:00:00Z"
}
```

**Error:**
```json
{
  "message": "Cannot delete a device that is currently in use."
}
```

## Domain Validations

The API enforces the following business rules:

| Rule | HTTP Status |
|------|-------------|
| **Creation time** cannot be updated | Enforced by design (private setter — immutable after creation) |
| **Name** and **Brand** cannot be updated if device is **in use** | `409 Conflict` |
| **In-use** devices cannot be deleted | `409 Conflict` |
| Device not found | `404 Not Found` |
| Invalid request payload | `400 Bad Request` |

## Running Tests

The project includes **76 tests** across three test categories:

```bash
# Run all tests
dotnet test

# Run with verbose output
dotnet test --verbosity normal

# Run only unit tests
dotnet test --filter "FullyQualifiedName~Unit"

# Run only integration tests
dotnet test --filter "FullyQualifiedName~Integration"
```

## Project Structure

```
test-task-devices-api/
├── src/
│   ├── DevicesApi.Domain/             # Domain Layer (Core — zero dependencies)
│   │   ├── Entities/
│   │   │   └── Device.cs              # Rich domain entity with behavior
│   │   ├── Enums/
│   │   │   └── DeviceState.cs         # Device state enumeration
│   │   ├── Exceptions/
│   │   │   ├── DomainException.cs     # Base domain exception
│   │   │   ├── DeviceNotFoundException.cs
│   │   │   ├── DeviceInUseException.cs
│   │   │   └── InvalidDeviceStateException.cs
│   │   └── Repositories/
│   │       └── IDeviceRepository.cs   # Repository port (interface)
│   │
│   ├── DevicesApi.Application/        # Application Layer (Use Cases)
│   │   ├── Commands/
│   │   │   ├── CreateDeviceCommand.cs
│   │   │   └── UpdateDeviceCommand.cs
│   │   ├── Results/
│   │   │   └── DeviceResult.cs
│   │   ├── Interfaces/
│   │   │   └── IDeviceService.cs
│   │   ├── Services/
│   │   │   └── DeviceService.cs
│   │   ├── Mappings/
│   │   │   └── DeviceMapper.cs
│   │   └── DependencyInjection.cs
│   │
│   ├── DevicesApi.Infrastructure/     # Infrastructure Layer (Persistence)
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Configurations/
│   │   │   │   └── DeviceConfiguration.cs
│   │   │   └── Repositories/
│   │   │       └── DeviceRepository.cs  # Repository adapter (implementation)
│   │   ├── Migrations/
│   │   └── DependencyInjection.cs
│   │
│   └── DevicesApi.Api/                # Presentation Layer (REST API)
│       ├── Controllers/
│       │   └── DevicesController.cs
│       ├── DTOs/
│       │   ├── CreateDeviceRequest.cs
│       │   ├── UpdateDeviceRequest.cs
│       │   └── DeviceResponse.cs
│       ├── Middleware/
│       │   └── ExceptionHandlingMiddleware.cs
│       ├── Program.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       └── appsettings.Production.json
│
├── tests/
│   └── DevicesApi.Tests/
│       ├── Unit/
│       │   ├── Domain/
│       │   │   └── DeviceEntityTests.cs      # Pure domain entity tests
│       │   └── Application/
│       │       └── DeviceServiceTests.cs     # Mocked service tests
│       └── Integration/
│           ├── CustomWebApplicationFactory.cs
│           └── DevicesControllerTests.cs
│
├── Dockerfile
├── docker-compose.yml
├── .dockerignore
├── .gitignore
├── DevicesApi.slnx
└── README.md
```

## Known Limitations

- **No pagination** — List endpoints return all matching records. With large datasets this will degrade performance and increase memory usage. Should add offset/limit or cursor-based pagination.
- **No authentication/authorization** — All endpoints are publicly accessible. A production deployment would need JWT or API key auth at minimum.
- **Single filter at a time** — The GET `/api/devices` endpoint applies brand OR state filtering, not both simultaneously. Combining filters would require a query object or specification pattern.
- **No concurrency handling** — The update flow (fetch → modify → save) has no optimistic concurrency control. Two concurrent updates to the same device could result in a lost update. Adding a `RowVersion`/`xmin` concurrency token to the entity would fix this.
- **Auto-migration on startup** — `Database.Migrate()` runs on every application start. This is convenient for development but risky in production with multiple instances (race conditions). A CI/CD migration step or init container would be safer.
- **In-memory DB for integration tests** — The InMemory provider doesn't enforce relational constraints (foreign keys, unique indexes). Some bugs that would appear with PostgreSQL won't surface in tests. Using Testcontainers with a real PostgreSQL instance would provide higher fidelity.
- **No request logging or correlation IDs** — Makes it harder to trace requests across logs in a distributed environment.
- **Hardcoded credentials** — Database username/password are in appsettings and docker-compose. In production these should come from a secret manager (Azure Key Vault, AWS Secrets Manager, etc.).

## Future Improvements

- **Pagination** — Add offset/limit or cursor-based pagination to list endpoints
- **Combined filtering** — Allow filtering by brand AND state simultaneously, possibly with a specification pattern
- **Optimistic concurrency** — Add a `RowVersion` column and EF Core concurrency token to prevent lost updates
- **Health checks** — Add `/health` endpoint with database connectivity check for container orchestration
- **Testcontainers** — Replace InMemory provider with a real PostgreSQL container in integration tests
- **API versioning** — Implement URL or header-based versioning for backward compatibility
- **Structured logging** — Integrate Serilog with correlation IDs and structured output
- **Authentication** — Add JWT-based auth with role-based access control
- **Rate limiting** — Protect endpoints from abuse using ASP.NET Core rate limiting middleware
- **Caching** — Add response caching or ETag support for read-heavy endpoints
- **CI/CD pipeline** — Configure GitHub Actions for automated build, test, and Docker image push
- **Audit trail** — Track who changed what and when, with soft-delete support
- **CQRS / MediatR** — Separate read and write paths as the domain grows in complexity
