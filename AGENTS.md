# AGENTS.md — rest-api-dotnet

## Quick Reference

**Stack**: .NET 10 LTS, C#, ASP.NET Core Minimal APIs, MySQL 9.7.2 (InnoDB, utf8mb4), RabbitMQ 4.3.6, Elasticsearch 9.5.4, Redis 8.8  
**Architecture**: Clean Architecture + Vertical Slice (Api, Application, Domain, Infrastructure, Contracts)  
**Pattern**: CQRS + Transactional Outbox; Elasticsearch is exclusive read model (no MySQL fallback)

---

## Essential Commands

### Run Full Stack (dev)

```bash
docker compose up --build
```

### Run API Only (requires infra running)

```bash
docker compose up --build rest-server
```

### Run Tests in Docker

```bash
docker compose --profile test build rest-test
docker compose --profile test run --rm rest-test
```

### Local Dev (outside Docker)

```bash
# Trust cert once
dotnet dev-certs https --clean && dotnet dev-certs https --trust

# Run API
dotnet run --project src/Backend.Api/Backend.Api.csproj
```

### Format / Build / Test Locally

```bash
dotnet format --verify-no-changes
dotnet build --warnaserror
dotnet test
```

---

## Architecture Rules (Non-Negotiable)

- **Commands** → write MySQL + outbox in **same transaction**
- **Queries** → read **only Elasticsearch** (never MySQL fallback)
- **Outbox**: one table per domain aggregate; processor publishes to RabbitMQ after commit
- **Redis**: sessions, revoked tokens, rate limiting only — **no user read model**
- **Passwords**: Argon2id only, never plain text
- **Program.cs** = composition root only; no business logic
- **Vertical Slice**: endpoints, commands, queries, domain, persistence, outbox in separate PascalCase files per feature

---

## Project Structure

```text
Backend.sln
src/
  Backend.Api/              # Minimal APIs, endpoint groups, middleware
  Backend.Application/      # Commands, Queries, Abstractions (ports)
  Backend.Domain/           # Entities, VOs, Events (no external deps)
  Backend.Infrastructure/   # MySQL, Elasticsearch, RabbitMQ, Redis, Security
  Backend.Contracts/        # Public request/response DTOs
tests/
  Backend.Application.Tests/     # Unit tests (xUnit)
  Backend.Api.IntegrationTests/  # HTTP tests (WebApplicationFactory)
```

---

## SDD Workflow (Required)

```text
spec (docs/specs/) → plan (docs/plans/) → tasks (docs/tasks/) → implement → test → update docs
```

When changing: infra, deps, Dockerfile, docker-compose.yml, env vars, build/test/run commands → update all affected `.md` files (README, docs/, plans, tasks, ADRs).

### Task Completion Tracking

Tasks in `docs/tasks/` use checkbox format. Mark completed tasks with `[x]`:

```markdown
- [x] Implement `GET /api/v1/access-users/{id}`.
- [ ] Implement `GET /api/v1/access-users` with pagination.
```

This provides clear visual progress tracking and ensures documentation stays synchronized with implementation.

---

## Key Constraints

- `Nullable` enabled, `TreatWarningsAsErrors` true (Directory.Build.props)
- Use `CancellationToken` everywhere I/O occurs
- Use `Result`/`ProblemDetails` for errors; no exceptions for control flow
- One class/responsibility per file (PascalCase)
- No artificial abstractions: interfaces = ports or real variations
- Migrations in `migrations/*.sql` (versioned, run via Docker entrypoint)
- Elasticsearch index bootstrap via `elasticsearch/init-access-users-index.sh` (runs before API)

---

## Test Notes

- Unit tests: `Backend.Application.Tests` (no external deps)
- Integration tests: `Backend.Api.IntegrationTests` (WebApplicationFactory; HTTP only)
- Infrastructure tests require full Compose stack
- Run single test: `dotnet test --filter "FullyQualifiedName~CreateAccessUserHandlerTests"`

---

## Environment Variables (Docker)

| Service | Key Variables |
| --------- | --------------- |
| MySQL | `MYSQL_ROOT_PASSWORD`, `MYSQL_DATABASE`, `MYSQL_USER`, `MYSQL_PASSWORD` |
| RabbitMQ | `RABBITMQ_DEFAULT_USER`, `RABBITMQ_DEFAULT_PASS` |
| API | `ASPNETCORE_URLS`, `ASPNETCORE_Kestrel__Certificates__Default__Path`, `ASPNETCORE_Kestrel__Certificates__Default__Password` |

---

## Language Rule

All code, docs, specs, plans, tasks, ADRs, comments = **English only** (C#, SQL, config, .md).