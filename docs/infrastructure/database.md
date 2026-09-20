# Infrastructure: MySQL

## Status

Approved for the first persistent implementation.

## Choice

- Database: MySQL 9.7.2.
- Storage engine: InnoDB in all domain and outbox tables.
- Charset: `utf8mb4`.
- Collation: explicitly defined per environment, preferring a deterministic `utf8mb4` collation.
- Access: approved ADO.NET/EF Core or micro-ORM provider for MySQL; write queries use explicit transactions.

> MySQL has a legacy `utf8` charset limited to 3 bytes. To support full Unicode, including emoji, the project must use `utf8mb4` in tables, connections, and migrations.

## Modeling Rules

- Every table must declare `ENGINE=InnoDB` and `CHARACTER SET=utf8mb4`.
- Primary and foreign keys must be indexed.
- Emails must have normalization and uniqueness defined in schema.
- Passwords must be stored only as hash.
- Dates must use a documented UTC convention.
- Schema changes must be versioned by reproducible migrations.

## Migrations And Bootstrap With Docker Compose

The stack must be started by `docker compose up --build`, and the environment must include SQL migration application before the API is ready for use.

Expected structure:

```text
migrations/
  001_create_access_users.sql
  002_create_access_users_outbox.sql
```

The bootstrap process must ensure:

- MySQL starts with a persistent volume;
- SQL scripts are applied in numbered order;
- domain and outbox tables are created in the same reproducible environment;
- API and other services wait for complete database initialization before receiving traffic.

Migrations must be versioned in the repository and executed by an init job or a migration container in `docker-compose.yml`.

## Outbox Per Table

Each domain table that produces events will have its own outbox table. For the access user, for example, the `access_users` table will be accompanied by `access_users_outbox`.

The domain record write and corresponding outbox record must occur in the same InnoDB transaction. The outbox must contain, at minimum, `event_id`, `aggregate_id`, `event_type`, `payload`, `status`, `attempts`, `available_at`, `created_at`, `published_at`, and `last_error`.

Each outbox can be processed independently, but all events must carry a globally unique identifier for idempotency in RabbitMQ and consumers.

## Tests

- Test migrations in real MySQL via Docker.
- Test rollback when outbox write fails.
- Test uniqueness, optimistic concurrency, and `utf8mb4` charset.
- Test that no password or secret is persisted in plain text.

---

**Language Rule**: All code, documentation, specifications, plans, tasks, ADRs, and comments must be written in English. This includes C# code, SQL, configuration files, and all `.md` files.