# Infraestrutura: RabbitMQ

## Status

Aprovada para a publicação dos eventos da Transactional Outbox.

## Responsabilidade

RabbitMQ será o broker de mensagens entre o processador das outboxes e os consumidores de eventos de integração. Ele não será a fonte de verdade dos dados de negócio; essa responsabilidade permanece no MySQL.

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

O processador somente deve publicar registros após o commit da transação que os criou. Depois de uma confirmação de publicação, o registro da outbox pode ser marcado como publicado.

## Bootstrap Local Com Docker Compose

O ambiente local deve subir com `docker compose up --build` e o RabbitMQ deve já iniciar com a configuração básica do broker:

- exchange `access_users.events`
- fila `access_users.events.queue`
- binding entre exchange e fila
- usuário administrativo padrão `guest` para ambiente local

A configuração deve estar versionada em `rabbitmq/rabbitmq.conf` e `rabbitmq/definitions.json`, e não depender de execução manual após a subida do contêiner.

## Regras

- Usar publisher confirms.
- Usar exchanges e routing keys versionadas por tipo de evento.
- Garantir entrega at-least-once.
- Configurar retry com backoff e dead-letter queue para mensagens que excederem o limite.
- Consumidores devem ser idempotentes pelo `event_id`.
- Não colocar senha, token ou dados sensíveis no payload sem decisão de segurança.
- Definir timeouts, limites de payload e política de durabilidade.

## Testes

- Publicação confirmada e falha de confirmação.
- Retry e dead-letter queue.
- Duplicata com o mesmo `event_id`.
- Reinício do RabbitMQ sem perda de registros ainda pendentes na outbox.
