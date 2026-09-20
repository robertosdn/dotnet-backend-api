# Documentação SDD

Esta pasta usa Specification-Driven Development (SDD).

- `constitution.md`: princípios e critérios obrigatórios.
- `architecture.md`: estrutura técnica e regras de CQRS e Transactional Outbox.
- `infrastructure/`: especificações operacionais de banco, fila, cache e observabilidade.
- `infrastructure/search.md`: especificação do Elasticsearch como read model.
- `decisions/`: ADRs com escolhas técnicas e suas consequências.
- `specs/`: comportamento esperado de cada funcionalidade.
- `plans/`: estratégia para implementar uma funcionalidade ou evolução.
- `tasks/`: checklist executável derivado do plano.

## Estrutura .NET

A solução usa .NET 10 LTS, C# e ASP.NET Core Minimal APIs:

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

`Backend.Api/Program.cs` é o composition root. As dependências são registradas por extensões de `IServiceCollection`, e os endpoints são organizados por feature em arquivos PascalCase. A API usa `ProblemDetails`, Minimal APIs, `CancellationToken` e `WebApplicationFactory` nos testes HTTP.

## Organização Dos Testes .NET

Este projeto centraliza os testes em projetos `*.Tests`, segmentados por responsabilidade. Não deve existir uma classe única concentrando toda a suíte.

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

Testes unitários ficam no projeto `Backend.Api.UnitTests` e testes de integração no projeto `Backend.Api.IntegrationTests`, usando xUnit e `WebApplicationFactory` quando aplicável. O padrão operacional é manter a suíte segmentada por camada, evitando classes de teste monolíticas e testes acoplados a detalhes privados.

Fluxo recomendado:

```text
spec -> plan -> tasks -> implementation -> tests -> update docs
```
