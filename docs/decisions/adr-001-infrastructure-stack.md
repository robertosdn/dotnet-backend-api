# ADR-001: Stack De Infraestrutura

## Status

Aceita.

## Contexto

A API backend precisa evoluir para features de negócio reais, iniciando pela gestão de usuários de acesso, com persistência, eventos de integração e cache sem misturar responsabilidades.

## Decisao

- Usar MySQL com InnoDB como fonte de verdade.
- Usar `utf8mb4` em tabelas, conexões e migrações para Unicode completo.
- Criar uma tabela de outbox por tabela de domínio que produza eventos.
- Usar RabbitMQ como broker para publicar eventos após o commit.
- Usar Elasticsearch como read model exclusivo das queries, inclusive consultas por id.
- Usar Redis somente para sessões, tokens revogados, rate limiting e dados temporários.
- Manter CQRS: commands escrevem no MySQL e outbox; queries leem somente do Elasticsearch.
- Usar Clean Architecture + Vertical Slice na solucao .NET 10, com `Backend.Api`, `Backend.Application`, `Backend.Domain`, `Backend.Infrastructure` e `Backend.Contracts`.
- Usar Minimal APIs versionadas em `/api/v1`, `ProblemDetails` para erros e IoC por extensoes de `IServiceCollection`.

## Consequencias Positivas

- Transações ACID para dados de negócio e eventos.
- Eventos desacoplados por RabbitMQ.
- Consultas especializadas e independentes do write model.
- Isolamento operacional entre outboxes de diferentes tabelas.

## Consequencias E Riscos

- A operação exige MySQL, RabbitMQ e Redis no ambiente local e de testes.
- A entrega da outbox é at-least-once, exigindo consumidores idempotentes.
- Outboxes por tabela aumentam o número de migrações e processadores a monitorar.
- A projeção para Elasticsearch exige consistência eventual, reprocessamento e observabilidade.
- O Redis exige TTL, revogação e monitoramento, mas não participa das queries de usuários.
- A separacao por projetos exige disciplina nos limites de dependencia e pode aumentar a quantidade de arquivos por feature.

## Fora Desta ADR

Esta decisao nao escolhe ainda o formato de token de login, o algoritmo de hash de senha, o pacote NuGet de persistencia ou as regras de autorizacao administrativa. Essas decisoes devem ser registradas antes da implementacao dos endpoints.
