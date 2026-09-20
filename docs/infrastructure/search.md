# Infrastructure: Elasticsearch

## Status

Approved as query read model.

## Responsibility

Elasticsearch will be the exclusive query mechanism for the access user API, including by-identifier lookups. MySQL remains the write model and source of truth for commands; Elasticsearch is a derived and rebuildable projection.

## Flow

```text
Command -> MySQL + Outbox -> RabbitMQ -> Elasticsearch index
Query   -> Elasticsearch
```

The consumer must project outbox events into the corresponding index after the MySQL commit is complete. Delivery and projection must be idempotent using the event identifier and aggregate version.

## Local Bootstrap With Docker Compose

The local environment must start with `docker compose up --build` and Elasticsearch must start with the basic API index predefined:

- index `access_users`
- mappings for `id`, `email`, `name`, `status`, `version`, `created_at`, and `updated_at`
- shards at 1 and replicas at 0 for local environment

Index creation must occur in a Docker Compose bootstrap step, so the API finds the read model ready on first use, without needing to create the mapping manually.

## Rules

- Queries cannot query MySQL as a fallback.
- Queries by `id`, filters, pagination, and text search must use Elasticsearch.
- Do not index `password_hash`, passwords, tokens, or secrets.
- Define mappings, aliases, versioning policy, and reindexing strategy.
- Configure replicas, refresh interval, timeouts, and pagination limits.
- Accept eventual consistency between a MySQL write and its availability in the index.
- Rebuild the index from MySQL via an operational process, without turning this rebuild into a query fallback.

## Failure Behavior

If Elasticsearch is unavailable, queries must return an observable read model unavailable error. The system must not silently query MySQL to mask the failure.

## Tests

- Projection of creation, update, and deactivation events.
- Idempotent projection and ordering by version.
- Query by id, filters, pagination, and text search.
- Elasticsearch failure without MySQL fallback.
- Reindexing and recovery after index loss.

---

**Language Rule**: All code, documentation, specifications, plans, tasks, ADRs, and comments must be written in English. This includes C# code, SQL, configuration files, and all `.md` files.