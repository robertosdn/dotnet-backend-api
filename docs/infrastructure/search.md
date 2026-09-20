# Infraestrutura: Elasticsearch

## Status

Aprovada como read model das queries.

## Responsabilidade

Elasticsearch será o mecanismo exclusivo de consulta da API de usuários de acesso, inclusive consultas por identificador. MySQL continua sendo o write model e a fonte de verdade dos commands; Elasticsearch é uma projeção derivada e reconstruível.

## Fluxo

```text
Command -> MySQL + Outbox -> RabbitMQ -> Elasticsearch index
Query   -> Elasticsearch
```

O consumidor deve projetar os eventos da outbox no índice correspondente depois que o commit do MySQL for concluído. A entrega e a projeção devem ser idempotentes usando o identificador do evento e a versão do agregado.

## Bootstrap Local Com Docker Compose

O ambiente local deve subir com `docker compose up --build` e o Elasticsearch deve iniciar com o índice básico da API predefinido:

- índice `access_users`
- mappings para `id`, `email`, `name`, `status`, `version`, `created_at` e `updated_at`
- shards em 1 e réplicas em 0 para ambiente local

A criação do índice deve ocorrer em um passo de bootstrap do Docker Compose, para que a API encontre o read model pronto no primeiro uso, sem necessidade de criar o mapeamento manualmente.

## Regras

- Queries não podem consultar MySQL como fallback.
- Consultas por `id`, filtros, paginação e busca textual devem usar Elasticsearch.
- Não indexar `password_hash`, senhas, tokens ou segredos.
- Definir mappings, aliases, política de versionamento e estratégia de reindexação.
- Configurar replicas, refresh interval, timeouts e limites de paginação.
- Aceitar consistência eventual entre uma escrita no MySQL e sua disponibilidade no índice.
- Reconstruir o índice a partir do MySQL por processo operacional, sem transformar essa reconstrução em fallback de query.

## Comportamento De Falhas

Se Elasticsearch estiver indisponível, as queries devem retornar erro observável de read model indisponível. O sistema não deve consultar MySQL silenciosamente para mascarar a falha.

## Testes

- Projeção de eventos de criação, alteração e desativação.
- Projeção idempotente e ordenação por versão.
- Consulta por id, filtros, paginação e busca textual.
- Falha do Elasticsearch sem fallback para MySQL.
- Reindexação e recuperação após perda do índice.
