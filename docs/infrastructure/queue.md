# Infraestrutura: RabbitMQ

## Status

Aprovada para a publicacao dos eventos da Transactional Outbox.

## Responsabilidade

RabbitMQ sera o broker de mensagens entre o processador das outboxes e os consumidores de eventos de integracao. Ele nao sera a fonte de verdade dos dados de negocio; essa responsabilidade permanece no MySQL.

## Fluxo

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

O processador somente deve publicar registros apos o commit da transacao que os criou. Depois de uma confirmacao de publicacao, o registro da outbox pode ser marcado como publicado.

## Bootstrap Local Com Docker Compose

O ambiente local deve subir com `docker compose up --build` e o RabbitMQ deve já iniciar com a configuracao basica do broker:

- exchange `access_users.events`
- fila `access_users.events.queue`
- binding entre exchange e fila
- usuario administrativo padrao `guest` para ambiente local

A configuracao deve estar versionada em `rabbitmq/rabbitmq.conf` e `rabbitmq/definitions.json`, e nao depender de execucao manual apos a subida do contêiner.

## Regras

- Usar publisher confirms.
- Usar exchanges e routing keys versionadas por tipo de evento.
- Garantir entrega at-least-once.
- Configurar retry com backoff e dead-letter queue para mensagens que excederem o limite.
- Consumidores devem ser idempotentes pelo `event_id`.
- Nao colocar senha, token ou dados sensiveis no payload sem decisao de seguranca.
- Definir timeouts, limites de payload e politica de durabilidade.

## Testes

- Publicacao confirmada e falha de confirmacao.
- Retry e dead-letter queue.
- Duplicata com o mesmo `event_id`.
- Reinicio do RabbitMQ sem perda de registros ainda pendentes na outbox.
