# Tasks: Access User Management

## Decisions And Foundation

- [x] Register MySQL/InnoDB, `utf8mb4`, and migration strategy.
- [x] Choose and register ADO.NET/EF Core or micro-ORM provider for MySQL persistence; keep RabbitMQ package choice pending until publisher task.
- [x] Register credential format, hash algorithm, and authorization policy.
- [x] Register RabbitMQ as broker and Redis for sessions and temporary data.
- [x] Register Elasticsearch as exclusive read model for queries.
- [x] Register one outbox table per domain table that produces events.
- [x] Define projects, namespaces, and contracts for commands, queries, domain, and infrastructure ports.
- [x] Define Clean Architecture + Vertical Slice, pragmatic SOLID, and IoC via `IServiceCollection`.
- [x] Define versioned Minimal API endpoints in `/api/v1` with `ProblemDetails` and OpenAPI.
- [x] Define user model, status, email uniqueness, and optimistic concurrency.

## Persistence And Outbox

- [x] Create user migration/table without storing password in plain text.
- [x] Create transactional outbox table per domain table, with status, attempts, and timestamps.
- [x] Implement real transactional unit in MySQL/InnoDB for user and outbox event.
- [x] Document and configure Docker Compose stack to start infrastructure and apply SQL migrations in bootstrap.
- [x] Document outbox processor with retry, backoff, and idempotency.
- [ ] Implement outbox processor with retry, backoff, and idempotency using real RabbitMQ.
- [x] Document RabbitMQ -> Elasticsearch projector.
- [ ] Implement RabbitMQ -> Elasticsearch projector using real RabbitMQ consumer.
- [x] Document Elasticsearch reindexing from MySQL without using MySQL in normal query path.
- [ ] Implement Elasticsearch reindexing from MySQL without using MySQL in normal query path.

## Endpoints And Use Cases

- [x] Create the project structure planned in `docs/plans/initial-architecture.md`: `Backend.Api`, `Backend.Application`, `Backend.Domain`, `Backend.Infrastructure`, and `Backend.Contracts`.
- [x] Implement `POST /api/v1/access-users` following `Endpoint -> Command -> domain -> MySQL/outbox repository`, with email, name, and password validation; unique email, Argon2id hash, transactional outbox event, and response without `password_hash`.
- [x] Implement `PATCH /api/v1/access-users/{id}`.
- [ ] Implement `GET /api/v1/access-users/{id}`.
- [ ] Implement `GET /api/v1/access-users` with pagination.
- [ ] Implement `POST /api/v1/auth/login`.
- [ ] Ensure responses and logs never expose password, hash, or token.

## Tests And Withdrawal

- [x] Verify that `src/Backend.Api/Program.cs` contains only composition root, middleware, DI, and routes, without domain rules, hash, persistence, or user state.
- [x] Create test projects `Backend.Api.UnitTests` and `Backend.Api.IntegrationTests`, segmented by layer.
- [x] Remove test suite concentration in a monolithic test class.
- [ ] Add domain tests in `Backend.Api.UnitTests/Domain/` for validation, normalization, password, and status transitions.
- [ ] Add command and query tests in `Backend.Api.UnitTests/Application/`, covering concurrency and rules without side effects.
- [ ] Add repository tests in `Backend.Api.IntegrationTests/Repositories/`, including uniqueness and atomicity with outbox in real MySQL.
- [ ] Add processor tests in `Backend.Api.IntegrationTests/Outbox/`, covering retry, backoff, and reprocessing after publication failure in real RabbitMQ.
- [ ] Add HTTP tests in `Backend.Api.IntegrationTests/Http/`, using `WebApplicationFactory` and covering endpoints, error codes, valid login, invalid, non-existent, and disabled user.
- [ ] Add tests for queries by id, filters, pagination, and search in Elasticsearch.
- [ ] Add test for Elasticsearch failure without MySQL fallback.
- [ ] Run persistence, outbox, and endpoint tests against real MySQL, RabbitMQ, Elasticsearch, and Redis via Docker Compose, including `dotnet format --verify-no-changes`, `dotnet test`, and `dotnet build --warnaserror`.
- [x] Remove routes and tests from legacy bootstrap features after new API is validated.
- [ ] Update specification, plan, and this list with results.
- [ ] Publish OpenAPI for endpoints and validate error responses with `ProblemDetails`.

## Completion Gate

No endpoint task can be marked as complete while the planned modular structure does not exist, while `Program.cs` concentrates domain, application, or infrastructure responsibilities, or while runtime uses `InMemoryAccessUserRepository`. Review must check file paths, contracts between layers, connection with MySQL/RabbitMQ, and corresponding tests.

An implementation can only be marked as complete when it uses the final environment infrastructure: MySQL/InnoDB for the write model, outbox in the same transaction, and RabbitMQ for post-commit publication. In-memory doubles are allowed only in isolated unit tests and cannot be registered as endpoint implementation.

---

**Language Rule**: All code, documentation, specifications, plans, tasks, ADRs, and comments must be written in English. This includes C# code, SQL, configuration files, and all `.md` files.