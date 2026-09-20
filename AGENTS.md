# Project Instructions

These instructions apply to people and any development agent used in this repository.

## Objective

Develop a complete backend API in .NET 10 LTS with C#, modular, testable, and prepared to evolve with multiple business features, including access user management.

## Mandatory Architecture

- Use CQRS: commands alter the write model; queries only consult the read model.
- Use MySQL with InnoDB and `utf8mb4` as write model and source of truth.
- Use Transactional Outbox: each domain table that produces events must have its own outbox, written atomically with the domain change.
- Use RabbitMQ to transport events after commit.
- Use Elasticsearch as the exclusive read model for queries, including by-id lookups. Queries must not query MySQL as a fallback.
- Use Redis only for sessions, revoked tokens, rate limiting, and temporary data. Redis does not participate in the user read model.

## Modularity

- Use Clean Architecture + Vertical Slice: separate `Backend.Api`, `Backend.Application`, `Backend.Domain`, `Backend.Infrastructure`, and `Backend.Contracts`.
- Separate domain, commands, queries, HTTP endpoints, persistence, outbox, projectors, and infrastructure into their own PascalCase classes and files.
- Apply SOLID pragmatically and use IoC via `IServiceCollection`; `Program.cs` must be only the composition root.
- Keep methods small, responsibilities clear, and components reusable.
- Avoid monolithic files and artificial abstractions.

## Security And Memory

- Never store or expose passwords in plain text.
- Use nullable reference types, `Result`/`ProblemDetails`, `async`/`await`, `CancellationToken`, and `IDisposable`/`IAsyncDisposable` to handle states and resources.
- Review concurrency, tasks, channels, locks, connections, cancellation tokens, and DI scopes.
- Look for incorrect nullability, deadlocks, logical data races, connection leaks, and unobserved tasks.

## Docker And Validation

- Use Docker Compose or Dockerfile stages to restore, build, test, and run .NET.
- Run applicable validations inside Docker, including `dotnet format --verify-no-changes`, `dotnet test`, and `dotnet build --warnaserror`.
- Do not consider a change complete without reporting the commands executed and their results.

## SDD And Synchronization

The Specification-Driven Development (SDD) pattern for this project is documented in [`docs/README.md`](docs/README.md). Consult the referenced documentation before implementing a feature.

Follow the flow:

```text
spec -> plan -> tasks -> implementation -> tests -> update docs
```

Whenever you change infrastructure, dependencies, `Dockerfile`, `docker-compose.yml`, environment variables, or build, test, and run commands:

- Update the corresponding `.md` files in `README.md`, `docs/`, plans, tasks, and applicable ADRs.
- Update all affected command examples.
- Update health checks, ports, volumes, and documented configurations when they change.
- Keep code, operational configuration, commands, and documentation synchronized.

## Changes

- Read the specification, plan, tasks, and related tests before editing.
- Make the smallest change consistent with the existing architecture.
- Add or update unit and integration tests for each changed behavior.
- Do not discard pre-existing changes or modify files outside scope without necessity.

---

**Language Rule**: All code, documentation, specifications, plans, tasks, ADRs, and comments must be written in English. This includes C# code, SQL, configuration files, and all `.md` files.