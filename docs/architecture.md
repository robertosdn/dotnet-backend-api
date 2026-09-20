# Arquitetura

## Estado Atual

A aplicacao e um servico HTTP em C# usando ASP.NET Core sobre .NET 10 LTS. O ponto de entrada esta em `src/Backend.Api/Program.cs` e a composicao das rotas e registrada na camada HTTP.

A gestao de usuarios de acesso e a primeira feature de negocio documentada em `docs/specs/access-user-management.md`, dentro do backend completo.

O Dockerfile possui etapas `test`, `build`, `publish` e `production`, que executam `dotnet test`, `dotnet publish` e hospedam a aplicacao. O Docker Compose expoe o servico em `8080` e possui um perfil `test` para a imagem de testes.

A infraestrutura aprovada esta definida em `docs/decisions/adr-001-infrastructure-stack.md`: MySQL 9.7.2 com InnoDB e `utf8mb4` como write model, RabbitMQ 4.3.6 como broker, Elasticsearch 9.5.4 como read model e Redis 8.8 somente para sessoes e dados temporarios.

## Direcao Arquitetural

A organizacao de codigo segue Clean Architecture combinada com Vertical Slice. Clean Architecture define os limites de dependencia; cada feature organiza seus casos de uso, contratos HTTP e testes por fluxo de negocio. SOLID e aplicado de forma pragmatica, sem criar interfaces para classes que nao possuem mais de uma implementacao ou um limite arquitetural claro.

```text
Backend.sln
    src/Backend.Api              -> composicao, endpoints e middleware
    src/Backend.Application      -> commands, queries e portas
    src/Backend.Domain           -> entidades, value objects e eventos
    src/Backend.Infrastructure   -> MySQL, Elasticsearch, RabbitMQ, Redis e seguranca
    src/Backend.Contracts        -> requests e responses publicos
    tests/Backend.*.Tests        -> testes por responsabilidade
```

As dependencias apontam para dentro: `Api` depende de `Application`, `Application` depende de `Domain` e `Infrastructure` implementa as abstracoes definidas por `Application`. `Domain` nao depende de ASP.NET Core, banco, broker, Elasticsearch ou Redis.

`Program.cs` e o composition root. Ele configura o host, middleware, OpenAPI, grupos de rotas e extensoes de `IServiceCollection`, mas nao contem regras de negocio. Cada modulo de infraestrutura expoe uma extensao como `AddPersistence`, `AddMessaging` ou `AddSecurity` para registrar seu proprio grafo de dependencias.

### Convencoes .NET 10

- Usar Minimal APIs com grupos de rota e endpoints tipados por feature.
- Usar `ProblemDetails` e `ValidationProblemDetails` para erros HTTP.
- Usar `CancellationToken` em endpoints, handlers, repositorios e workers.
- Usar `BackgroundService` para o processador da outbox.
- Usar `WebApplicationFactory` nos testes HTTP de integracao.
- Usar PascalCase em arquivos e classes: `CreateAccessUserEndpoint.cs`, `CreateAccessUserCommand.cs` e `MySqlAccessUserWriteRepository.cs`.
- Manter uma classe, record, interface ou responsabilidade principal por arquivo.

O formato publico da API permanece versionado e orientado a recursos:

```text
POST   /api/v1/access-users
GET    /api/v1/access-users/{id}
GET    /api/v1/access-users
PATCH  /api/v1/access-users/{id}
POST   /api/v1/auth/login
```

As novas funcionalidades devem seguir as camadas abaixo:

```text
HTTP handler
    |
    +--> Command handler --> Domain --> Write repository + Outbox repository
    |
    +--> Query handler   --> Elasticsearch read model

Outbox processor --> Event publisher --> Elasticsearch projector
```

O fluxo HTTP detalhado e `Endpoint -> Command/Query -> Handler -> Domain/Port -> Adapter`. Endpoints apenas convertem HTTP em request, chamam o caso de uso e convertem o resultado em status HTTP.

### Commands

Commands representam intencoes que podem alterar estado. O command handler valida a entrada, executa a regra de dominio e grava a alteracao junto com o evento de integracao na mesma transacao.

### Queries

Queries somente leem o read model do Elasticsearch. Nao devem consultar MySQL como fallback, alterar estado, criar eventos ou depender de efeitos colaterais de commands.

### Transactional Outbox

A tabela ou armazenamento da outbox deve conter, no minimo, identificador do evento, tipo, payload, status, tentativas, timestamps e erro da ultima tentativa. Cada tabela de dominio que produzir eventos tera sua propria tabela de outbox. A alteracao de dominio e o registro da outbox devem ser confirmados atomicamente no MySQL/InnoDB.

Um processador separado busca eventos pendentes, publica cada evento no RabbitMQ e marca o registro como processado. O processamento deve ter retry, backoff, idempotencia e observabilidade. A publicacao deve ser at-least-once; consumidores precisam aceitar duplicatas.

Os adaptadores de producao devem usar MySQL/InnoDB e RabbitMQ reais. O endpoint de criacao usa um provider ADO.NET/EF Core ou micro-ORM aprovado para gravar usuario e outbox na mesma transacao; o publisher RabbitMQ sera conectado pelo worker do processador da outbox. Implementacoes em memoria sao permitidas somente para testes unitarios e nao podem ser registradas no DI de runtime como substitutas da infraestrutura final.

### Redis Temporario

Redis nao participa do read model de CQRS. Deve ser usado somente para sessoes, tokens revogados, rate limiting e outros dados temporarios com TTL explicito. MySQL continua sendo o write model e Elasticsearch o read model.

## Restricoes

- Nao publicar eventos diretamente antes do commit da transacao.
- Nao misturar leitura e escrita no mesmo handler sem justificativa documentada.
- Nao introduzir banco, broker ou pacote NuGet de persistencia sem atualizar esta arquitetura e a especificacao da funcionalidade.
- Manter o contrato HTTP existente salvo quando uma especificacao aprovada definir a mudanca.
- Registrar implementacoes no IoC por extensoes de `IServiceCollection`, evitando service locator e dependencias concretas nos handlers.
- Nao criar abstracoes artificiais: uma interface deve representar uma porta, uma politica ou uma variacao real de infraestrutura.
