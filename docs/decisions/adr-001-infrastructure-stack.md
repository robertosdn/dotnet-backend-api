# ADR-001: Stack De Infraestrutura

## Status

Aceita.

## Contexto

A API backend precisa evoluir dos endpoints de demonstracao para features de negocio reais, iniciando pela gestao de usuarios de acesso, com persistencia, eventos de integracao e cache sem misturar responsabilidades.

## Decisao

- Usar MySQL com InnoDB como fonte de verdade.
- Usar `utf8mb4` em tabelas, conexoes e migracoes para Unicode completo.
- Criar uma tabela de outbox por tabela de dominio que produza eventos.
- Usar RabbitMQ como broker para publicar eventos apos o commit.
- Usar Elasticsearch como read model exclusivo das queries, inclusive consultas por id.
- Usar Redis somente para sessoes, tokens revogados, rate limiting e dados temporarios.
- Manter CQRS: commands escrevem no MySQL e outbox; queries leem somente do Elasticsearch.
- Usar Clean Architecture + Vertical Slice na solucao .NET 10, com `Backend.Api`, `Backend.Application`, `Backend.Domain`, `Backend.Infrastructure` e `Backend.Contracts`.
- Usar Minimal APIs versionadas em `/api/v1`, `ProblemDetails` para erros e IoC por extensoes de `IServiceCollection`.

## Consequencias Positivas

- Transacoes ACID para dados de negocio e eventos.
- Eventos desacoplados por RabbitMQ.
- Consultas especializadas e independentes do write model.
- Isolamento operacional entre outboxes de diferentes tabelas.

## Consequencias E Riscos

- A operacao exige MySQL, RabbitMQ e Redis no ambiente local e de testes.
- A entrega da outbox e at-least-once, exigindo consumidores idempotentes.
- Outboxes por tabela aumentam o numero de migracoes e processadores a monitorar.
- A projecao para Elasticsearch exige consistencia eventual, reprocessamento e observabilidade.
- O Redis exige TTL, revogacao e monitoramento, mas nao participa das queries de usuarios.
- A separacao por projetos exige disciplina nos limites de dependencia e pode aumentar a quantidade de arquivos por feature.

## Fora Desta ADR

Esta decisao nao escolhe ainda o formato de token de login, o algoritmo de hash de senha, o pacote NuGet de persistencia ou as regras de autorizacao administrativa. Essas decisoes devem ser registradas antes da implementacao dos endpoints.
