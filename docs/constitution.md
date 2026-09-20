# Project Constitution

## Objective

Maintain a complete backend API in C# on .NET 10 LTS, testable, and prepared to evolve with multiple features without losing architectural clarity.

## Mandatory Principles

1. **Idiomatic C#/.NET**: prefer strong types, nullable reference types, explicit errors, async/await, and small responsibilities.
2. **CQRS**: separate operations that alter state (commands) from operations that only query state (queries).
3. **Transactional Outbox**: when an operation alters state and generates an integration event, persist both in the same atomic transaction before publishing the event.
4. **Idempotency**: commands and outbox processing must tolerate retries without duplicating effects.
5. **Segmented Tests**: every new or changed feature must have unit tests and, when applicable, integration tests in separate classes in `*.Tests` projects; do not concentrate the suite in a monolithic class.
6. **Docker**: build, test, and run the application preferably via Docker Compose or Dockerfile stages.
7. **Small Changes**: avoid out-of-scope refactoring and preserve existing contracts.
8. **Verifiable Modularity**: responsibilities must be in their own projects, namespaces, and files per the plan; `Program.cs` is the composition point, not a place for business rules or infrastructure.
9. **Pragmatic SOLID**: apply single responsibility, dependency inversion, and interfaces only when they represent ports, policies, or real variations.
10. **Explicit IoC**: register dependencies via `IServiceCollection` extensions; avoid service locator, global state, and concrete dependencies in use cases.
11. **Idiomatic .NET API**: use Minimal APIs, versioned groups in `/api/v1`, `ProblemDetails`, OpenAPI, and `CancellationToken` in I/O operations.
12. **Clean Usings**: no unused `using` directives in any `.cs` file; rely on `ImplicitUsings` and remove self-namespace or unreferenced imports.

## Definition Of Done

A change is only complete when:

- the corresponding specification has been updated;
- commands, queries, and events are separated as applicable;
- the planned project structure exists and `Program.cs` contains only host composition and dependency/route registration;
- projects depend in a single direction and `Backend.Domain` does not depend on ASP.NET Core or infrastructure;
- relevant unit and integration tests have been added or updated;
- tests are segmented by layer in `*.Tests` projects, without a monolithic test class;
- `dotnet format --verify-no-changes`, `dotnet test`, and `dotnet build --warnaserror` have been executed in Docker when supported by the environment;
- the build reports zero `IDE0005`/`CS8019` violations (no unused `using` directives);
- risks, limitations, and executed commands have been recorded in the change summary.

---

**Language Rule**: All code, documentation, specifications, plans, tasks, ADRs, and comments must be written in English. This includes C# code, SQL, configuration files, and all `.md` files.