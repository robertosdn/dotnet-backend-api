# Infraestrutura: MySQL

## Status

Aprovada para a primeira implementacao persistente.

## Escolha

- Banco: MySQL 9.7.2.
- Storage engine: InnoDB em todas as tabelas de dominio e outbox.
- Charset: `utf8mb4`.
- Collation: definir explicitamente por ambiente, preferindo uma collation `utf8mb4` deterministica.
- Acesso: provider ADO.NET/EF Core ou micro-ORM aprovado para MySQL; as queries de escrita usam transacoes explicitas.

> O MySQL possui o charset historico `utf8` limitado a 3 bytes. Para suportar Unicode completo, inclusive emoji, o projeto deve usar `utf8mb4` nas tabelas, conexoes e migracoes.

## Regras De Modelagem

- Toda tabela deve declarar `ENGINE=InnoDB` e `CHARACTER SET=utf8mb4`.
- Chaves primarias e estrangeiras devem ser indexadas.
- Emails devem ter normalizacao e unicidade definidas no schema.
- Senhas devem ser armazenadas somente como hash.
- Datas devem usar uma convencao UTC documentada.
- Alteracoes de schema devem ser versionadas por migracoes reproduziveis.

## Migrações E Bootstrap Com Docker Compose

A stack deve ser iniciada por `docker compose up --build`, e o ambiente deve incluir a aplicacao das migracoes SQL antes da API ficar pronta para uso.

Estrutura esperada:

```text
migrations/
  001_create_access_users.sql
  002_create_access_users_outbox.sql
```

O processo de bootstrap deve garantir que:

- o MySQL suba com volume persistente;
- os scripts SQL sejam aplicados em ordem numerada;
- tabelas de dominio e outbox sejam criadas no mesmo ambiente reproduzivel;
- a API e os demais servicos esperem a inicializacao completa do banco antes de receber trafego.

As migracoes devem ser versionadas no repositorio e executadas por um job de init ou por um container de migracao no `docker-compose.yml`.

## Outbox Por Tabela

Cada tabela de dominio que produzir eventos tera sua propria tabela de outbox. Para o usuario de acesso, por exemplo, a tabela `access_users` sera acompanhada de `access_users_outbox`.

A escrita do registro de dominio e do registro correspondente na outbox deve ocorrer na mesma transacao InnoDB. A outbox deve conter, no minimo, `event_id`, `aggregate_id`, `event_type`, `payload`, `status`, `attempts`, `available_at`, `created_at`, `published_at` e `last_error`.

Cada outbox pode ser processada independentemente, mas todos os eventos devem carregar um identificador globalmente unico para idempotencia no RabbitMQ e nos consumidores.

## Testes

- Testar migracoes em banco MySQL real via Docker.
- Testar rollback quando a escrita da outbox falhar.
- Testar unicidade, concorrencia otimista e charset `utf8mb4`.
- Testar que nenhuma senha ou segredo e persistido em texto puro.
