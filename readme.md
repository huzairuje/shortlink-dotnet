# MyFirstApi — URL Shortener

A URL shortener REST API built with ASP.NET Core 8, following Clean Architecture principles. Data is stored in-memory and persisted to a local JSON file on shutdown and periodically every minute.

## Architecture

```
MyFirstApi/
├── Core/                          # Business logic — zero external dependencies
│   ├── Entities/
│   │   └── Link.cs                # Domain entity
│   ├── Ports/
│   │   ├── Repositories/
│   │   │   └── ILinkRepository.cs
│   │   └── Services/
│   │       └── ILinkService.cs
│   └── UseCases/
│       └── LinkService.cs         # Application logic
│
├── Infrastructure/                # Adapters — implements ports
│   ├── Persistence/
│   │   ├── InMemoryLinkRepository.cs
│   │   ├── JsonPersistenceService.cs
│   │   └── PersistenceBackgroundService.cs
│   └── DependencyInjection.cs
│
└── Api/                           # HTTP layer
    ├── DTOs/
    │   └── LinkDtos.cs
    ├── Endpoints/
    │   └── LinkEndpoints.cs
    └── Program.cs
```

### Layer responsibilities

| Layer | Responsibility | Can depend on |
|---|---|---|
| `Core` | Business rules, domain entities, port interfaces | Nothing |
| `Infrastructure` | Persistence, external services, port implementations | `Core` |
| `Api` | HTTP routing, request/response mapping, DI wiring | `Core`, `Infrastructure` |

## Requirements

- .NET 8 SDK

```bash
dotnet --version
# 8.x.x
```

## Getting started

```bash
git clone <repo-url>
cd MyFirstApi

dotnet restore
dotnet run
```

Server runs at `http://localhost:5000`. Swagger UI available at `http://localhost:5000/swagger`.

## API endpoints

All responses use `snake_case` JSON.

### Create a short link

```
POST /links
```

```json
// Request
{
  "originalUrl": "https://example.com",
  "customSlug": "my-link"   // optional — auto-generated if omitted or empty
}

// Response 201 Created
{
  "slug": "my-link",
  "original_url": "https://example.com",
  "short_url": "http://localhost:5000/my-link",
  "created_at": "2026-05-04T09:02:26.767Z",
  "clicks": 0
}
```

**Error responses**

| Status | Reason |
|---|---|
| `400 Bad Request` | `original_url` is missing or empty |
| `409 Conflict` | `custom_slug` already exists |
| `422 Unprocessable Entity` | Domain validation error |

### List all links

```
GET /links
```

```json
// Response 200 OK
[
  {
    "slug": "my-link",
    "original_url": "https://example.com",
    "short_url": "http://localhost:5000/my-link",
    "created_at": "2026-05-04T09:02:26.767Z",
    "clicks": 3
  }
]
```

### Get a link by slug

```
GET /links/{slug}
```

Returns `404 Not Found` if slug does not exist.

### Redirect

```
GET /{slug}
```

Redirects (`302`) to the original URL and increments the click counter. Returns `404 Not Found` if slug does not exist.

### Delete a link

```
DELETE /links/{slug}
```

Returns `204 No Content` on success, `404 Not Found` if slug does not exist.

## Persistence

Data is kept in memory while the app is running and persisted to `links.json` in two ways:

- **Periodic** — auto-saved every 1 minute via `PersistenceBackgroundService`
- **On shutdown** — saved when the app receives `SIGTERM` (Ctrl+C)

> **Note:** Hard-killing the process (`kill -9`) bypasses the shutdown hook. Use Ctrl+C to stop the server gracefully.

On startup, `links.json` is read and data is restored to memory automatically.

`links.json` is excluded from version control via `.gitignore`.

## Development notes

### Environment variable (SDK 10 only)

If running with .NET SDK 10 preview, set this before running:

```bash
export AllowMissingPrunePackageData=true
dotnet run
```

Or add it permanently to `~/.zshrc` / `~/.bashrc`:

```bash
echo 'export AllowMissingPrunePackageData=true' >> ~/.zshrc
source ~/.zshrc
```

### Hot reload

```bash
dotnet watch run
```

### Test with curl

```bash
# Create
curl -X POST http://localhost:5000/links \
  -H "Content-Type: application/json" \
  -d '{"originalUrl": "https://github.com"}'

# Create with custom slug
curl -X POST http://localhost:5000/links \
  -H "Content-Type: application/json" \
  -d '{"originalUrl": "https://github.com", "customSlug": "gh"}'

# List all
curl http://localhost:5000/links

# Get by slug
curl http://localhost:5000/links/gh

# Redirect
curl -L http://localhost:5000/gh

# Delete
curl -X DELETE http://localhost:5000/links/gh
```

## Key concepts (for Go developers)

| Go | C# |
|---|---|
| `interface` in `ports/` | `ILinkRepository`, `ILinkService` |
| Implicit interface implementation | Explicit (`: ILinkRepository`) |
| `sync.RWMutex` | `ReaderWriterLockSlim` |
| Manual wire in `main.go` | `DependencyInjection.cs` + built-in DI container |
| `context.Context` cancellation | `CancellationToken` |
| `goroutine` + `ticker` | `BackgroundService` + `Task.Delay` |