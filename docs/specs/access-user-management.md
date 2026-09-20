# Specification: Access User Management

## Status

Planned.

## Objective

Implement a real API to create, update, query, and authenticate access users.

## Conceptual Model

An access user has, at minimum:

- immutable `id`;
- unique and normalized `email`;
- `name` or display name;
- `password_hash`, never the password in plain text;
- `status`, initially `active` or `disabled`;
- creation and update timestamps;
- version control or other optimistic concurrency strategy.

The HTTP response must never expose `password_hash`.

## Endpoints

### Create User

`POST /api/v1/access-users`

- Command: `CreateAccessUser`.
- Must validate email, name, and password policy.
- Must reject duplicate email with `409 Conflict`.
- Must store only a password hash with an appropriate algorithm.
- Must return `201 Created` without password or hash in response.
- Must record an `AccessUserCreated` event in the outbox in the same transaction as creation.

### Update User

`PATCH /api/v1/access-users/{id}`

- Command: `UpdateAccessUser`.
- Must allow changing name, email, and status according to authorization rules.
- Email change must preserve uniqueness and normalization.
- Password change must generate a new hash and never store the original password.
- Must return `200 OK` without password or hash.
- Must record an integration event only when there is a relevant state change.

### Get User

`GET /api/v1/access-users/{id}`

- Query: `GetAccessUser`.
- Must return `200 OK` with public user data.
- Must return `404 Not Found` when user does not exist.
- Must not alter state or create event.

### List Users

`GET /api/v1/access-users`

- Query: `ListAccessUsers`.
- Must support deterministic pagination.
- Must allow documented filters, such as status and email.
- Must not return password or hash.
- Must not alter state or create event.

### Login

`POST /api/v1/auth/login`

- Command: `LoginAccessUser` or authentication use case with credential reading.
- Must locate user by normalized email and verify password hash.
- Must reject invalid credentials with a generic response, without revealing if email exists.
- Must reject `disabled` user.
- On success, must return a session credential or token per recorded architectural decision.
- Must not log password, token, or credential.
- Login attempts and security events must follow outbox policy when there are integration consumers.

## CQRS And Outbox

Commands alter state in MySQL and queries only read the read model in Elasticsearch. Queries must never query MySQL as a fallback. Creation, update, deactivation, and security events must use a transactional unit. User write and outbox event registration must be committed atomically.

The outbox processor must publish events with at-least-once delivery, retry with backoff, and idempotent behavior. The algorithm must follow: select `pending` and `available_at <= now` events, attempt to publish to RabbitMQ, record attempt, and on error, increase `attempts`, calculate exponential delay, and mark `last_error`. Successful events must be marked as `published` and maintain idempotency by `event_id` to avoid duplication on reprocessing.

The RabbitMQ -> Elasticsearch projector must consume events published by the user aggregate and apply changes to the read model, preserving `id`, `email`, `name`, `status`, `version`, and timestamps. Indexing must be idempotent by user `id` and never depend on MySQL queries. Consumers must accept duplicates using the event identifier.

Elasticsearch reindexing from MySQL is an infrastructure and maintenance operation, not part of the normal API read path. When the read model needs rebuilding, a reindexing job queries MySQL in batch, rewrites documents in Elasticsearch, and invalidates or replaces relevant indices. API queries continue 100% in Elasticsearch and never query MySQL as a fallback.

## Modules And Contracts

The components below are implementation requirements and must be created as separate C# projects, namespaces, and files. The organization combines Clean Architecture with Vertical Slice: architectural boundaries are in projects and each feature groups endpoint, use case, contracts, and related tests.

### Organization By Aggregate

Files belonging directly to the `AccessUser` aggregate must be grouped by feature and namespace within the responsible layer, avoiding polluting the folder with files from other aggregates. Expected organization includes:

```text
src/
  Backend.Api/
    Endpoints/AccessUsers/AccessUserEndpointGroup.cs
    Endpoints/AccessUsers/CreateAccessUserEndpoint.cs
    Endpoints/AccessUsers/UpdateAccessUserEndpoint.cs
    Endpoints/AccessUsers/GetAccessUserEndpoint.cs
    Endpoints/AccessUsers/ListAccessUsersEndpoint.cs
    Endpoints/Authentication/LoginEndpoint.cs
  Backend.Application/
    AccessUsers/Commands/CreateAccessUser/CreateAccessUserCommand.cs
    AccessUsers/Commands/CreateAccessUser/CreateAccessUserHandler.cs
    AccessUsers/Queries/GetAccessUser/GetAccessUserQuery.cs
    AccessUsers/Queries/GetAccessUser/GetAccessUserHandler.cs
    Abstractions/Persistence/IAccessUserWriteRepository.cs
    Abstractions/Persistence/IAccessUserReadRepository.cs
    Abstractions/Messaging/IEventPublisher.cs
  Backend.Domain/
    AccessUsers/AccessUser.cs
    AccessUsers/AccessUserId.cs
    AccessUsers/AccessUserEmail.cs
    AccessUsers/AccessUserStatus.cs
    AccessUsers/AccessUserCreatedEvent.cs
    AccessUsers/AccessUserUpdatedEvent.cs
  Backend.Infrastructure/
    Persistence/MySql/MySqlAccessUserWriteRepository.cs
    Persistence/Elasticsearch/ElasticsearchAccessUserReadRepository.cs
    Messaging/RabbitMq/RabbitMqEventPublisher.cs
    Messaging/RabbitMq/RabbitMqOutboxProcessor.cs
    Security/Argon2PasswordHasher.cs
    Security/JwtTokenService.cs
  Backend.Contracts/
    AccessUsers/CreateAccessUserRequest.cs
    AccessUsers/AccessUserResponse.cs
```

The same criteria must be applied to new aggregates and domain components. `Outbox/OutboxRecord.cs` and `Outbox/Processor.cs` remain in the cross-cutting layer; the `Outbox.AccessUser` namespace concentrates events specific to this aggregate. Grouping classes without their own responsibility must not be created just to form namespaces.

Implementation must follow module division to preserve low coupling and maintain project rules:

- `Domain`: entities, value objects, enums, and internal rules. E.g., `AccessUser`, `AccessUserStatus`, `EmailAddress`, `UserId`, `PasswordHash`.
- `Application/Commands`: write commands. E.g., `CreateAccessUserCommand`, `UpdateAccessUserCommand`, `LoginAccessUserCommand`, each receiving a DTO and returning a typed result with validation or domain errors.
- `Application/Queries`: read queries. E.g., `GetAccessUserQuery`, `ListAccessUsersQuery`, always returning public views without side effects.
- `Application/Abstractions`: ports for write, read, messaging, and security. `IAccessUserWriteRepository` defines `InsertAsync`, `UpdateAsync`, `FindByIdAsync`, `FindByEmailAsync`, and `SaveOutboxEventAsync`; `IAccessUserReadRepository` defines search by id, filters, and pagination in Elasticsearch.
- `Outbox`: transactional event entity and `BackgroundService` with retry, backoff, idempotency, and observability.
- `Auth`: password validation, token issuance and validation, and policies/middleware for role-based authorization.
- `Api/Endpoints`: Minimal APIs, route groups, and conversion between HTTP and use cases, without embedded domain logic.

### C# File Convention

Each class, record, interface, or main component must be in a corresponding PascalCase file matching its responsibility. In infrastructure, for example, `MySqlAccessUserRepository` is in `Infrastructure/MySql/MySqlAccessUserRepository.cs`, `ElasticsearchAccessUserReadRepository` in `Infrastructure/Elasticsearch/ElasticsearchAccessUserReadRepository.cs`, `RabbitMqEventPublisher` in `Infrastructure/RabbitMq/RabbitMqEventPublisher.cs`, and `RedisSessionStore` in `Infrastructure/Redis/RedisSessionStore.cs`.

Method names must reflect the operation or domain event they execute, with enough specificity to not confuse responsibilities. Use .NET conventions like `CreateAsync`, `FindByIdAsync`, `PublishAsync`, and `HandleAsync`, always with `CancellationToken` when there is I/O.

When an entity or component grows, each own domain responsibility must be in its own class or C# file. For example, `AccessUserCreatedEvent` implementation is in `Outbox/AccessUser/AccessUserCreatedEvent.cs`, while `Outbox/OutboxRecord.cs` remains reserved for the `OutboxRecord` model and its states.

For the `AccessUserUpdated` event, the same pattern requires `AccessUserUpdatedEvent` in `Outbox/AccessUser/AccessUserUpdatedEvent.cs`. Different events must have different domain payloads, even when sharing the same fields, and the outbox must accept both without altering the JSON payload contracts.

`src/Backend.Api/Program.cs` is outside these boundaries as the application composition root. It can register routes, middleware, and dependencies in IoC, but cannot implement validation, hash, user rules, persistence, outbox, or state storage. The creation endpoint will only be considered implemented when it respects the chaining `Endpoint -> Application/Commands -> Domain/Port -> Infrastructure/Adapter`.

Contracts must maintain a clear boundary: endpoints transform HTTP into commands/queries, application handlers execute validation and domain, and infrastructure adapters are the only data exchange with MySQL/Elasticsearch/Redis.

## User Model And Status

The access user domain model must follow the structure below:

- `id`: UUIDv7 or database-generated bigint identifier, immutable;
- `email`: string normalized to lowercase, no spaces, validated by canonical format;
- `name`: display name with character limit and length validation;
- `password_hash`: Argon2id hash, never stored in plain text;
- `status`: enum `active | disabled`;
- `created_at` and `updated_at`: audit timestamps;
- `version`: version number for optimistic concurrency.

The MySQL `access_users` table must follow the schema:

```sql
CREATE TABLE access_users (
  id BINARY(16) PRIMARY KEY,
  email VARCHAR(254) NOT NULL,
  name VARCHAR(255) NOT NULL,
  password_hash VARCHAR(255) NOT NULL,
  status ENUM('active', 'disabled') NOT NULL DEFAULT 'active',
  version BIGINT NOT NULL DEFAULT 0,
  created_at DATETIME(6) NOT NULL,
  updated_at DATETIME(6) NOT NULL,
  UNIQUE KEY uk_access_users_email (email)
) ENGINE=InnoDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

The MySQL `access_users_outbox` table must register each event generated by user write in the same transaction:

```sql
CREATE TABLE access_users_outbox (
  id BINARY(16) PRIMARY KEY,
  aggregate_id BINARY(16) NOT NULL,
  event_type VARCHAR(100) NOT NULL,
  payload JSON NOT NULL,
  status ENUM('pending', 'published', 'failed') NOT NULL DEFAULT 'pending',
  attempts INT NOT NULL DEFAULT 0,
  available_at DATETIME(6) NOT NULL,
  created_at DATETIME(6) NOT NULL,
  published_at DATETIME(6) NULL,
  last_error VARCHAR(1000) NULL,
  KEY ix_access_users_outbox_status_available (status, available_at)
) ENGINE=InnoDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

Domain rules:

- Email is unique across active and inactive users and must be queried in normalized form.
- `disabled` status blocks login and disables authentication operations, but does not remove the record.
- User `PATCH` must update `updated_at` and increment `version` only when there is a relevant change.
- Write operations must reject concurrency conditions when `version` provided by client diverges from the database record.

## Transactional Unit Of User And Event

The `AccessUser` aggregate change and outbox event registration must happen within the same InnoDB transaction. The integration contract must be:

```text
begin transaction
  insert/update access_users
  insert access_users_outbox
commit
```

If any step fails, the entire transaction must be rolled back. In terms of application design:

- the command handler validates input and creates the domain event;
- the write repository persists the entity and outbox record in the same transactional unit;
- the outbox processor only publishes events after successful commit;
- the API does not publish events outside the transaction, nor write domain events in HTTP handlers.

This ensures consistency between state and integration, without allowing the user to be altered without a validated event being registered.

## Security

- Never store or return password in plain text.
- Use a reviewed password hashing library with configurable parameters.
- Apply input validation and size limits.
- Avoid user enumeration in login.
- Define authentication and authorization for administrative operations before releasing the API to production.
- Define rate limiting and attempt blocking policy before exposing login publicly.

## Recorded Decisions

- Database and persistence provider: the architectural decision remains `MySQL/InnoDB` with `utf8mb4` and a dedicated C#/.NET repository, per infrastructure ADR.
- Credential format: use stateless JWT access token with `sub`, `role`, and `exp`, signed with `RS256`. The access token expires in 15 minutes and a refresh token, when it exists, is stored in Redis with TTL for revocation and fast invalidation.
- Authorization rules: `admin` can create, update, list, and disable any user; `user` can query their own profile and update only non-sensitive data of their own account; anonymous cannot access management endpoints. Every operation validates the token role in each request.
- Hash algorithm: use `Argon2id` with configurable parameters (memory 64 MiB, time cost 3, parallelism 2), storing only `password_hash` and never the password in plain text. Password rotation policy requires rehash when detecting old parameters or when password is changed.
- Rate limiting and attempt blocking strategy: apply limit by IP and by email for login, with exponential backoff and temporary blocking in Redis; login responses must remain generic to avoid enumeration.

## Mandatory Tests

Tests must be kept in `Backend.Api.UnitTests` and `Backend.Api.IntegrationTests` projects, segmented by responsibility. The suite must not be concentrated in a monolithic class.

Expected structure:

```text
tests/
  Backend.Api.UnitTests/
  Backend.Api.IntegrationTests/
```

- Unit tests for validation, normalization, password, status transitions, and domain rules.
- Command handler tests for duplication, concurrency, and atomicity with outbox.
- Query tests without side effects and without hash exposure.
- Tests for successful login, invalid password, non-existent user, and disabled user.
- HTTP tests for contracts, status codes, and response formats.
- Outbox processor tests for retry, idempotency, and publication failure.

---

**Language Rule**: All code, documentation, specifications, plans, tasks, ADRs, and comments must be written in English. This includes C# code, SQL, configuration files, and all `.md` files.