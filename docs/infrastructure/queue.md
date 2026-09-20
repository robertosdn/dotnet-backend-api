# Infrastructure: RabbitMQ

## Status

Approved for Transactional Outbox event publication.

## Responsibility

RabbitMQ will be the message broker between outbox processors and integration event consumers. It will not be the source of truth for business data; that responsibility remains with MySQL.

## Flow

```text
MySQL domain table + table_outbox
              |
              v
       Outbox processor
              |
              v
            RabbitMQ
              |
              v
           Consumers
```

The processor must only publish records after the transaction that created them is committed. After a publication confirmation, the outbox record can be marked as published.

## Local Bootstrap With Docker Compose

The local environment must start with `docker compose up --build` and RabbitMQ must already start with the basic broker configuration:

- exchange `access_users.events`
- queue `access_users.events.queue`
- binding between exchange and queue
- default administrative user `guest` for local environment

Configuration must be versioned in `rabbitmq/rabbitmq.conf` and `rabbitmq/definitions.json`, and must not depend on manual execution after container startup.

## Rules

- Use publisher confirms.
- Use versioned exchanges and routing keys by event type.
- Guarantee at-least-once delivery.
- Configure retry with backoff and dead-letter queue for messages exceeding the limit.
- Consumers must be idempotent by `event_id`.
- Do not put password, token, or sensitive data in payload without security decision.
- Define timeouts, payload limits, and durability policy.

## Tests

- Confirmed publication and confirmation failure.
- Retry and dead-letter queue.
- Duplicate with same `event_id`.
- RabbitMQ restart without loss of records still pending in outbox.

---

**Language Rule**: All code, documentation, specifications, plans, tasks, ADRs, and comments must be written in English. This includes C# code, SQL, configuration files, and all `.md` files.