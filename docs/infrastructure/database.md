# Infraestrutura: MySQL

## Status

Aprovada para a primeira implementação persistente.

## Escolha

- Banco: MySQL 9.7.2.
- Storage engine: InnoDB em todas as tabelas de domínio e outbox.
- Charset: `utf8mb4`.
- Collation: definir explicitamente por ambiente, preferindo uma collation `utf8mb4` determinística.
- Acesso: provider ADO.NET/EF Core ou micro-ORM aprovado para MySQL; as queries de escrita usam transações explícitas.

> O MySQL possui o charset histórico `utf8` limitado a 3 bytes. Para suportar Unicode completo, inclusive emoji, o projeto deve usar `utf8mb4` nas tabelas, conexões e migrações.

## Regras De Modelagem

- Toda tabela deve declarar `ENGINE=InnoDB` e `CHARACTER SET=utf8mb4`.
- Chaves primárias e estrangeiras devem ser indexadas.
- E-mails devem ter normalização e unicidade definidas no schema.
- Senhas devem ser armazenadas somente como hash.
- Datas devem usar uma convenção UTC documentada.
- Alterações de schema devem ser versionadas por migrações reproduzíveis.

## Migrações E Bootstrap Com Docker Compose

A stack deve ser iniciada por `docker compose up --build`, e o ambiente deve incluir a aplicação das migrações SQL antes de a API ficar pronta para uso.

Estrutura esperada:

```text
migrations/
  001_create_access_users.sql
  002_create_access_users_outbox.sql
```

O processo de bootstrap deve garantir que:

- o MySQL suba com volume persistente;
- os scripts SQL sejam aplicados em ordem numerada;
- tabelas de domínio e outbox sejam criadas no mesmo ambiente reproduzível;
- a API e os demais serviços esperem a inicialização completa do banco antes de receber tráfego.

As migrações devem ser versionadas no repositório e executadas por um job de init ou por um container de migração no `docker-compose.yml`.

## Outbox Por Tabela

Cada tabela de domínio que produzir eventos terá sua própria tabela de outbox. Para o usuário de acesso, por exemplo, a tabela `access_users` será acompanhada de `access_users_outbox`.

A escrita do registro de domínio e do registro correspondente na outbox deve ocorrer na mesma transação InnoDB. A outbox deve conter, no mínimo, `event_id`, `aggregate_id`, `event_type`, `payload`, `status`, `attempts`, `available_at`, `created_at`, `published_at` e `last_error`.

Cada outbox pode ser processada independentemente, mas todos os eventos devem carregar um identificador globalmente único para idempotência no RabbitMQ e nos consumidores.

## Testes

- Testar migrações em banco MySQL real via Docker.
- Testar rollback quando a escrita da outbox falhar.
- Testar unicidade, concorrência otimista e charset `utf8mb4`.
- Testar que nenhuma senha ou segredo é persistido em texto puro.
