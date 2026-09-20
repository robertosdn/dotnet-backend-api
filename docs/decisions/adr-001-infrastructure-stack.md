# ADR-001: Infrastructure Stack

## Status

Accepted.

## Context

The backend API needs to evolve to real business features, starting with access user management, with persistence, integration events, and cache without mixing responsibilities.

## Decision

- Use MySQL with InnoDB as source of truth.
- Use `utf8mb4` in tables, connections, and migrations for full Unicode.
- Create one outbox table per domain table that produces events.
- Use RabbitMQ as broker to publish events after commit.
- Use Elasticsearch as exclusive read model for queries, including by-id lookups.
- Use Redis only for sessions, revoked tokens, rate limiting, and temporary data.
- Maintain CQRS: commands write to MySQL and outbox; queries read only from Elasticsearch.
- Use Clean Architecture + Vertical Slice in .NET 10 solution, with `Backend.Api`, `Backend.Application`, `Backend.Domain`, `Backend.Infrastructure`, and `Backend.Contracts`.
- Use versioned Minimal APIs in `/api/v1`, `ProblemDetails` for errors, and IoC via `IServiceCollection` extensions.

## Positive Consequences

- ACID transactions for business data and events.
- Decoupled events via RabbitMQ.
- Specialized queries independent of write model.
- Operational isolation between outboxes of different tables.

## Consequences And Risks

- Operation requires MySQL, RabbitMQ, and Redis in local and test environments.
- Outbox delivery is at-least-once, requiring idempotent consumers.
- Outboxes per table increase the number of migrations and processors to monitor.
- Projection to Elasticsearch requires eventual consistency, reprocessing, and observability.
- Redis requires TTL, revocation, and monitoring, but does not participate in user queries.
- Separation by projects requires discipline in dependency boundaries and may increase the number of files per feature.

## Out Of Scope For This ADR

This decision does not yet choose the login token format, password hash algorithm, persistence NuGet package, or administrative authorization rules. These decisions must be recorded before endpoint implementation.

---

**Language Rule**: All code, documentation, specifications, plans, tasks, ADRs, and comments must be written in English. This includes C# code, SQL, configuration files, and all `.md` files.