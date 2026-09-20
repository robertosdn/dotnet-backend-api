# SDD Documentation

This folder uses Specification-Driven Development (SDD).

- `constitution.md`: mandatory principles and criteria.
- `architecture.md`: technical structure and CQRS and Transactional Outbox rules.
- `infrastructure/`: operational specifications for database, queue, cache, and observability.
- `infrastructure/search.md`: Elasticsearch specification as read model.
- `decisions/`: ADRs with technical choices and their consequences.
- `specs/`: expected behavior of each feature.
- `plans/`: strategy to implement a feature or evolution.
- `tasks/`: executable checklist derived from the plan.

## .NET Structure

The solution uses .NET 10 LTS, C#, and ASP.NET Core Minimal APIs:

```text
Backend.sln
src/Backend.Api
src/Backend.Application
src/Backend.Domain
src/Backend.Infrastructure
src/Backend.Contracts
tests/Backend.Domain.Tests
tests/Backend.Application.Tests
tests/Backend.Infrastructure.IntegrationTests
tests/Backend.Api.IntegrationTests
```

`Backend.Api/Program.cs` is the composition root. Dependencies are registered via `IServiceCollection` extensions, and endpoints are organized by feature in PascalCase files. The API uses `ProblemDetails`, Minimal APIs, `CancellationToken`, and `WebApplicationFactory` in HTTP tests.

## .NET Test Organization

This project centralizes tests in `*.Tests` projects, segmented by responsibility. There must not be a single class concentrating the entire suite.

Minimum expected structure:

```text
tests/
  Backend.Api.UnitTests/
    Domain/AccessUserTests.cs
    Application/CommandTests.cs
  Backend.Api.IntegrationTests/
    Repositories/AccessUserWriteTests.cs
    Outbox/ProcessorTests.cs
    Http/AccessUsersTests.cs
    Fixtures/CustomWebApplicationFactory.cs
```

Unit tests are in the `Backend.Api.UnitTests` project and integration tests in `Backend.Api.IntegrationTests`, using xUnit and `WebApplicationFactory` when applicable. The operational pattern is to keep the suite segmented by layer, avoiding monolithic test classes and tests coupled to private details.

Recommended flow:

```text
spec -> plan -> tasks -> implementation -> tests -> update docs
```

---

**Language Rule**: All code, documentation, specifications, plans, tasks, ADRs, and comments must be written in English. This includes C# code, SQL, configuration files, and all `.md` files.