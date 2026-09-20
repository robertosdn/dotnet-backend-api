# Arquitetura

## Estado Atual

A aplicação é um serviço HTTP em C# usando ASP.NET Core sobre .NET 10 LTS. O ponto de entrada está em `src/Backend.Api/Program.cs` e a composição das rotas é registrada na camada HTTP.

A gestão de usuários de acesso é a primeira feature de negócio documentada em `docs/specs/access-user-management.md`, dentro do backend completo.

O Dockerfile possui etapas `test`, `build`, `publish` e `production`, que executam `dotnet test`, `dotnet publish` e hospedam a aplicação. O Docker Compose expõe o serviço em `8080` e possui um perfil `test` para a imagem de testes.

A infraestrutura aprovada está definida em `docs/decisions/adr-001-infrastructure-stack.md`: MySQL 9.7.2 com InnoDB e `utf8mb4` como write model, RabbitMQ 4.3.6 como broker, Elasticsearch 9.5.4 como read model e Redis 8.8 somente para sessões e dados temporários.

## Direção Arquitetural

A organização de código segue Clean Architecture combinada com Vertical Slice. Clean Architecture define os limites de dependência; cada feature organiza seus casos de uso, contratos HTTP e testes por fluxo de negócio. SOLID é aplicado de forma pragmática, sem criar interfaces para classes que não possuem mais de uma implementação ou um limite arquitetural claro.

```text
Backend.sln
    src/Backend.Api              -> composicao, endpoints e middleware
    src/Backend.Application      -> commands, queries e portas
    src/Backend.Domain           -> entidades, value objects e eventos
    src/Backend.Infrastructure   -> MySQL, Elasticsearch, RabbitMQ, Redis e seguranca
    src/Backend.Contracts        -> requests e responses publicos
    tests/Backend.*.Tests        -> testes por responsabilidade
```

As dependências apontam para dentro: `Api` depende de `Application`, `Application` depende de `Domain` e `Infrastructure` implementa as abstrações definidas por `Application`. `Domain` não depende de ASP.NET Core, banco, broker, Elasticsearch ou Redis.

`Program.cs` é o composition root. Ele configura o host, middleware, OpenAPI, grupos de rotas e extensões de `IServiceCollection`, mas não contém regras de negócio. Cada módulo de infraestrutura expõe uma extensão como `AddPersistence`, `AddMessaging` ou `AddSecurity` para registrar seu próprio grafo de dependências.

### Convenções .NET 10

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

O fluxo HTTP detalhado é `Endpoint -> Command/Query -> Handler -> Domain/Port -> Adapter`. Endpoints apenas convertem HTTP em request, chamam o caso de uso e convertem o resultado em status HTTP.

### Commands

Commands representam intenções que podem alterar estado. O command handler valida a entrada, executa a regra de domínio e grava a alteração junto com o evento de integração na mesma transação.

### Queries

Queries somente leem o read model do Elasticsearch. Não devem consultar MySQL como fallback, alterar estado, criar eventos ou depender de efeitos colaterais de commands.

### Transactional Outbox

A tabela ou armazenamento da outbox deve conter, no mínimo, identificador do evento, tipo, payload, status, tentativas, timestamps e erro da última tentativa. Cada tabela de domínio que produzir eventos terá sua própria tabela de outbox. A alteração de domínio e o registro da outbox devem ser confirmados atomicamente no MySQL/InnoDB.

Um processador separado busca eventos pendentes, publica cada evento no RabbitMQ e marca o registro como processado. O processamento deve ter retry, backoff, idempotência e observabilidade. A publicação deve ser at-least-once; consumidores precisam aceitar duplicatas.

Os adaptadores de produção devem usar MySQL/InnoDB e RabbitMQ reais. O endpoint de criação usa um provider ADO.NET/EF Core ou micro-ORM aprovado para gravar usuário e outbox na mesma transação; o publisher RabbitMQ será conectado pelo worker do processador da outbox. Implementações em memória são permitidas somente para testes unitários e não podem ser registradas no DI de runtime como substitutas da infraestrutura final.

### Redis Temporário

Redis não participa do read model de CQRS. Deve ser usado somente para sessões, tokens revogados, rate limiting e outros dados temporários com TTL explícito. MySQL continua sendo o write model e Elasticsearch o read model.

## Restrições

- Não publicar eventos diretamente antes do commit da transação.
- Não misturar leitura e escrita no mesmo handler sem justificativa documentada.
- Não introduzir banco, broker ou pacote NuGet de persistência sem atualizar esta arquitetura e a especificação da funcionalidade.
- Manter o contrato HTTP existente salvo quando uma especificação aprovada definir a mudança.
- Registrar implementações no IoC por extensões de `IServiceCollection`, evitando service locator e dependências concretas nos handlers.
- Não criar abstrações artificiais: uma interface deve representar uma porta, uma política ou uma variação real de infraestrutura.
