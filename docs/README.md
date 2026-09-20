# Documentacao SDD

Esta pasta usa Specification-Driven Development (SDD).

- `constitution.md`: principios e criterios obrigatorios.
- `architecture.md`: estrutura tecnica e regras de CQRS e Transactional Outbox.
- `infrastructure/`: especificacoes operacionais de banco, fila, cache e observabilidade.
- `infrastructure/search.md`: especificacao do Elasticsearch como read model.
- `decisions/`: ADRs com escolhas tecnicas e suas consequencias.
- `specs/`: comportamento esperado de cada funcionalidade.
- `plans/`: estrategia para implementar uma funcionalidade ou evolucao.
- `tasks/`: checklist executavel derivado do plano.

## Estrutura .NET

A solucao usa .NET 10 LTS, C# e ASP.NET Core Minimal APIs:

```text
Backend.sln
src/Backend.Api
src/Backend.Application
src/Backend.Domain
src/Backend.Infrastructure
src/Backend.Contracts
tests/Backend.Domain.Tests
tests/Backend.Application.Tests
tests/Backend.Infrastructure.IntegrationTests
tests/Backend.Api.IntegrationTests
```

`Backend.Api/Program.cs` e o composition root. As dependencias sao registradas por extensoes de `IServiceCollection`, e os endpoints sao organizados por feature em arquivos PascalCase. A API usa `ProblemDetails`, Minimal APIs, `CancellationToken` e `WebApplicationFactory` nos testes HTTP.

## Organizacao Dos Testes .NET

Este projeto centraliza os testes em projetos `*.Tests`, segmentados por responsabilidade. Nao deve existir uma classe unica concentrando toda a suite.

Estrutura minima esperada:

```text
tests/
  Backend.Api.UnitTests/
    Domain/AccessUserTests.cs
    Application/CommandTests.cs
  Backend.Api.IntegrationTests/
    Repositories/AccessUserWriteTests.cs
    Outbox/ProcessorTests.cs
    Http/AccessUsersTests.cs
    Fixtures/CustomWebApplicationFactory.cs
```

Testes unitarios ficam no projeto `Backend.Api.UnitTests` e testes de integracao no projeto `Backend.Api.IntegrationTests`, usando xUnit e `WebApplicationFactory` quando aplicavel. O padrao operacional e manter a suite segmentada por camada, evitando classes de teste monoliticas e testes acoplados a detalhes privados.

Fluxo recomendado:

```text
spec -> plan -> tasks -> implementation -> tests -> update docs
```
