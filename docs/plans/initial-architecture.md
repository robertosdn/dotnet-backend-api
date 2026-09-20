# Plan: Backend Evolution
## Objective
Implement the first real backend features, starting with access user management while maintaining CQRS, Transactional Outbox, credential security, and automated tests.
## Contracts And Planned Projects
This structure is mandatory for implementation, not just a suggested organization. Each responsibility must exist in the indicated project before the corresponding stage is marked as complete. `src/Backend.Api/Program.cs` must remain limited to host composition, DI, and route registration; it cannot contain entities, value objects, validation rules, password hashing, repository access, state storage, or command orchestration.
The initial backend structure must follow the separation below, maintaining CQRS and transactional outbox:
### Expected Contracts
### Implementation Rule
For each new endpoint, implementation must follow the flow `Endpoint -> Command/Query -> Handler -> Domain/Port -> Adapter`, with contracts defined in separate files. The endpoint only deserializes input, calls the handler, and converts the result to HTTP response. An implementation cannot be accepted if domain or persistence rules are concentrated in `Program.cs` or a single monolithic file.
Tests must also follow modular separation. The `Backend.Api.UnitTests` and `Backend.Api.IntegrationTests` projects are mandatory for the main suite, with separate classes for domain, commands/queries, repositories, outbox, and HTTP. A single test class does not meet the plan. Unit tests must use xUnit and HTTP tests must use `WebApplicationFactory` when necessary.
## Stages
1. Close open decisions from the access user management specification, including database, credential, hash, authorization, and event destination.
2. Choose database and .NET access package/provider, documenting the architectural decision.
3. Create `Backend.Api`, `Backend.Application`, `Backend.Domain`, `Backend.Infrastructure`, and `Backend.Contracts` projects with dependencies pointing inward.
4. Create the outbox model and transactional unit that writes user and event atomically.
5. Implement user creation, update, and queries with real MySQL/Elasticsearch and segmented unit and integration tests in `*.Tests` projects by layer.
6. Implement login with secure password verification, session/token credential, and responses that avoid enumeration.
7. Implement the outbox processor with retry, backoff, idempotency, and observability, with pending event selection, RabbitMQ publication attempt, error marking, and controlled reprocessing.
8. Implement the RabbitMQ -> Elasticsearch projector to materialize the user read model without MySQL queries in normal queries.
9. Implement emergency Elasticsearch reindexing from MySQL in batch, keeping the normal query path only in Elasticsearch.
10. Update Docker Compose with database and other necessary dependencies, apply SQL migrations in bootstrap, and validate endpoints against real infrastructure via the .NET test profile.
11. Consolidate the router with only business endpoints after real use case coverage.
At each implementation stage, review must verify the file tree, dependency boundaries between projects/namespaces, infrastructure effectively used by runtime, and existence of tests for the changed layer in `*.Tests` projects. The task can only be marked complete when this verification passes; in-memory mocks do not replace MySQL, RabbitMQ, or Elasticsearch in integration flows.
## Out Of Scope
- Do not implement administrative authentication without defining minimum authorization.
- Do not choose database, credential format, or broker without a recorded decision.
- Do not publish events directly from HTTP handlers.
## Exit Criteria