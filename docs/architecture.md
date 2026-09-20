# Architecture

## Current State

The application is an HTTP service in C# using ASP.NET Core on .NET 10 LTS. The entry point is in `src/Backend.Api/Program.cs` and route composition is registered in the HTTP layer.

Access user management is the first documented business feature in `docs/specs/access-user-management.md`, within the complete backend.

The Dockerfile has `test`, `build`, `publish`, and `production` stages that run `dotnet test`, `dotnet publish`, and host the application. Docker Compose exposes the service on `8080` and has a `test` profile for the test image.

Approved infrastructure is defined in `docs/decisions/adr-001-infrastructure-stack.md`: MySQL 9.7.2 with InnoDB and `utf8mb4` as write model, RabbitMQ 4.3.6 as broker, Elasticsearch 9.5.4 as read model, and Redis 8.8 only for sessions and temporary data.

## Architectural Direction

Code organization follows Clean Architecture combined with Vertical Slice. Clean Architecture defines dependency boundaries; each feature organizes its use cases, HTTP contracts, and tests by business flow. SOLID is applied pragmatically, without creating interfaces for classes that don't have more than one implementation or a clear architectural boundary.

```text
Backend.sln
    src/Backend.Api              -> composition, endpoints, and middleware
    src/Backend.Application      -> commands, queries, and ports
    src/Backend.Domain           -> entities, value objects, and events
    src/Backend.Infrastructure   -> MySQL, Elasticsearch, RabbitMQ, Redis, and security
    src/Backend.Contracts        -> public requests and responses
    tests/Backend.*.Tests        -> tests by responsibility
```

Dependencies point inward: `Api` depends on `Application`, `Application` depends on `Domain`, and `Infrastructure` implements abstractions defined by `Application`. `Domain` does not depend on ASP.NET Core, database, broker, Elasticsearch, or Redis.

`Program.cs` is the composition root. It configures the host, middleware, OpenAPI, route groups, and `IServiceCollection` extensions, but contains no business rules. Each infrastructure module exposes an extension like `AddPersistence`, `AddMessaging`, or `AddSecurity` to register its own dependency graph.

### .NET 10 Conventions

- Use Minimal APIs with route groups and typed endpoints by feature.
- Use `ProblemDetails` and `ValidationProblemDetails` for HTTP errors.
- Use `CancellationToken` in endpoints, handlers, repositories, and workers.
- Use `BackgroundService` for the outbox processor.
- Use `WebApplicationFactory` in HTTP integration tests.
- Use PascalCase for files and classes: `CreateAccessUserEndpoint.cs`, `CreateAccessUserCommand.cs`, and `MySqlAccessUserWriteRepository.cs`.
- Keep one class, record, interface, or main responsibility per file.

The public API format remains versioned and resource-oriented:

```text
POST   /api/v1/access-users
GET    /api/v1/access-users/{id}
GET    /api/v1/access-users
PATCH  /api/v1/access-users/{id}
POST   /api/v1/auth/login
```

New features must follow the layers below:

```text
HTTP handler
    |
    +--> Command handler --> Domain --> Write repository + Outbox repository
    |
    +--> Query handler   --> Elasticsearch read model

Outbox processor --> Event publisher --> Elasticsearch projector
```

The detailed HTTP flow is `Endpoint -> Command/Query -> Handler -> Domain/Port -> Adapter`. Endpoints only convert HTTP to request, call the use case, and convert the result to HTTP status.

### Commands

Commands represent intentions that can alter state. The command handler validates input, executes the domain rule, and writes the change along with the integration event in the same transaction.

### Queries

Queries only read the Elasticsearch read model. They must not query MySQL as a fallback, alter state, create events, or depend on command side effects.

### Transactional Outbox

The outbox table or storage must contain, at minimum, event identifier, type, payload, status, attempts, timestamps, and last attempt error. Each domain table that produces events will have its own outbox table. The domain change and outbox registration must be committed atomically in MySQL/InnoDB.

A separate processor fetches pending events, publishes each event to RabbitMQ, and marks the record as processed. Processing must have retry, backoff, idempotency, and observability. Publication must be at-least-once; consumers must accept duplicates.

Production adapters must use real MySQL/InnoDB and RabbitMQ. The creation endpoint uses an approved ADO.NET/EF Core provider or micro-ORM to write user and outbox in the same transaction; the RabbitMQ publisher will be connected by the outbox processor worker. In-memory implementations are allowed only for unit tests and cannot be registered in runtime DI as substitutes for the final infrastructure.

### Temporary Redis

Redis does not participate in the CQRS read model. It must be used only for sessions, revoked tokens, rate limiting, and other temporary data with explicit TTL. MySQL remains the write model and Elasticsearch the read model.

## Constraints

- Do not publish events directly before the transaction commit.
- Do not mix read and write in the same handler without documented justification.
- Do not introduce database, broker, or persistence NuGet package without updating this architecture and the feature specification.
- Maintain the existing HTTP contract unless an approved specification defines the change.
- Register implementations in IoC via `IServiceCollection` extensions, avoiding service locator and concrete dependencies in handlers.
- Do not create artificial abstractions: an interface must represent a port, a policy, or a real infrastructure variation.

---

**Language Rule**: All code, documentation, specifications, plans, tasks, ADRs, and comments must be written in English. This includes C# code, SQL, configuration files, and all `.md` files.