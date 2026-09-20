# Infrastructure: Redis

## Status

Approved only for sessions and temporary data, outside the CQRS read model.

## Responsibility

Redis will be used for sessions, revoked tokens, rate limiting, and other temporary data. Redis will not be used for user read model queries and will never be the source of truth for users or transactions.

## Rules

- Every key must have a namespace and version, e.g., `session:v1:{id}`.
- Every entry must have an explicit TTL, unless a documented decision states otherwise.
- The system must remain correct when Redis is unavailable.
- Redis unavailability must only block the temporary resource that depends on it; user queries must not attempt to use it.
- Session invalidation or update must occur according to the session lifecycle.
- Do not store passwords in plain text.
- Secrets, tokens, and sessions must have expiration and revocation policy.

## Consistency

MySQL remains the write model and Elasticsearch the read model. Redis is not part of the user query path.

## Tests

- Session creation, renewal, revocation, and expiration.
- Redis unavailable without corrupting persisted data.
- Rate limiting and revoked tokens, when implemented.
- Rate limiting and credential revocation, when implemented.

---

**Language Rule**: All code, documentation, specifications, plans, tasks, ADRs, and comments must be written in English. This includes C# code, SQL, configuration files, and all `.md` files.