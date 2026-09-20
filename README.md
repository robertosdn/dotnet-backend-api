# REST API .NET

Complete backend API in C# with ASP.NET Core on .NET 10 LTS. Access user management is one of the planned backend features.

## Architecture

The solution uses Clean Architecture + Vertical Slice in .NET 10 LTS. Projects are `Backend.Api`, `Backend.Application`, `Backend.Domain`, `Backend.Infrastructure`, and `Backend.Contracts`; `Program.cs` is the composition root and dependencies are registered via IoC through `IServiceCollection`.

The API uses Minimal APIs, endpoints organized by feature, PascalCase for files/classes, versioning `/api/v1`, OpenAPI, and `ProblemDetails` for error responses.

- **CQRS**: commands alter the write model; queries consult the read model.
- **MySQL 9.7.2 / InnoDB**: write model and source of truth.
- **Transactional Outbox**: one dedicated outbox per domain table that produces events.
- **RabbitMQ 4.3.6**: event transport after commit.
- **Elasticsearch 9.5.4**: exclusive read model for queries, including by-id lookups.
- **Redis 8.8**: sessions, revoked tokens, rate limiting, and temporary data only; does not participate in user queries.
- **.NET 10 LTS**: runtime and SDK for the application.
- **Docker**: standard environment for restore, build, tests, and execution.

Development flow:

```text
spec -> plan -> tasks -> implementation -> tests -> update docs
```

## Run With Docker

The complete backend stack must start via Docker Compose, including database, queue, search, cache services, bootstrap configurations, and initialization scripts so the local environment is functional from the first `up`.

Start the full development stack:

```bash
docker compose up --build
```

Start only the current HTTP service:

```bash
docker compose up --build backend-server
```

Run tests in the Docker test stage:

```bash
docker compose --profile test build backend-test
docker compose --profile test run --rm backend-test
```

The `backend-test` service runs the .NET test suite in isolation and does not start MySQL, RabbitMQ, Elasticsearch, or Redis. HTTP tests replace external ports with doubles; infrastructure tests must use the full Compose stack.

The API is available at `http://localhost:8080`.

### Local Development

Run the API locally:

```bash
dotnet run --project src/Backend.Api/Backend.Api.csproj
```

### Bootstrap in Docker Compose

- MySQL SQL migrations must be in `migrations/*.sql` and versioned in the repository.
- The Docker Compose environment must include automatic initialization for MySQL, RabbitMQ, and Elasticsearch before the API is ready for use.
- RabbitMQ bootstrap must create basic exchange, queue, and bindings for the outbox flow.
- Elasticsearch bootstrap must create initial indices and mappings, such as `access_users`, without relying on the app to create the schema at runtime.
- Stack bootstrap must ensure `access_users`, `access_users_outbox`, event queue, and query index are created automatically when infrastructure starts.
- The application must not depend on runtime-generated SQL in HTTP handlers; schema must be applied by reproducible migration.

## Specification-Driven Development

Before implementing a feature:

1. Update or create the specification in `docs/specs/`.
2. Record technical decisions in `docs/decisions/`.
3. Update the plan in `docs/plans/`.
4. Derive tasks in `docs/tasks/`.
5. Implement in modules separated by responsibility.
6. Add unit and integration tests.
7. Update documentation and run Docker validations.

## Documentation

- [`AGENTS.md`](AGENTS.md): shared instructions for people and agents of any tool.
- [`docs/README.md`](docs/README.md): SDD documentation index.
- [`docs/constitution.md`](docs/constitution.md): principles and Definition of Done.
- [`docs/architecture.md`](docs/architecture.md): architecture and write/read model boundaries.
- [`docs/specs/access-user-management.md`](docs/specs/access-user-management.md): user management API specification.
- [`docs/infrastructure/`](docs/infrastructure/): MySQL, RabbitMQ, Elasticsearch, and Redis.
- [`docs/decisions/`](docs/decisions/): ADRs and consequences of choices.
- [`docs/plans/`](docs/plans/): implementation plans.
- [`docs/tasks/`](docs/tasks/): executable checklists.

## Development Rules

- Keep commands, queries, domain, persistence, handlers, and projectors in separate modules.
- Do not query MySQL in the normal query path; use Elasticsearch.
- Do not publish events before the transaction commit.
- Never store or expose passwords in plain text.
- Create tests for each new or changed behavior.
- Review nullability, concurrency, DI lifecycles, cancellation, and possible resource leaks.
- Keep every `.cs` file free of unused `using` directives (`IDE0005`/`CS8019` fail the build); rely on `ImplicitUsings` and run `dotnet format` before committing.

---

**Language Rule**: All code, documentation, specifications, plans, tasks, ADRs, and comments must be written in English. This includes C# code, SQL, configuration files, and all `.md` files.