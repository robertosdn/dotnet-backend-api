# Especificação: Gestão de Usuário de Acesso

## Status

Planejada.

## Objetivo

Implementar uma API real para criar, alterar, consultar e autenticar usuários de acesso.

## Modelo Conceitual

Um usuário de acesso possui, no mínimo:

- `id` imutavel;
- `email` único e normalizado;
- `name` ou nome de exibição;
- `password_hash`, nunca a senha em texto puro;
- `status`, inicialmente `active` ou `disabled`;
- timestamps de criação e alteração;
- controle de versão ou outra estratégia de concorrência otimista.

A resposta HTTP nunca deve expor `password_hash`.

## Endpoints

### Criar usuário

`POST /api/v1/access-users`

- Command: `CreateAccessUser`.
- Deve validar e-mail, nome e política de senha.
- Deve rejeitar e-mail duplicado com `409 Conflict`.
- Deve armazenar somente um hash de senha com algoritmo apropriado.
- Deve retornar `201 Created` sem senha ou hash na resposta.
- Deve registrar um evento `AccessUserCreated` na outbox na mesma transação da criação.

### Alterar usuário

`PATCH /api/v1/access-users/{id}`

- Command: `UpdateAccessUser`.
- Deve permitir alterar nome, e-mail e status conforme as regras de autorização.
- Alteração de e-mail deve preservar unicidade e normalização.
- Alteração de senha deve gerar novo hash e nunca armazenar a senha original.
- Deve retornar `200 OK` sem senha ou hash.
- Deve registrar evento de integração somente quando houver mudança de estado relevante.

### Consultar usuário

`GET /api/v1/access-users/{id}`

- Query: `GetAccessUser`.
- Deve retornar `200 OK` com dados publicos do usuario.
- Deve retornar `404 Not Found` quando o usuário não existir.
- Não deve alterar estado nem criar evento.

### Listar usuários

`GET /api/v1/access-users`

- Query: `ListAccessUsers`.
- Deve suportar paginação determinística.
- Deve permitir filtros documentados, como status e e-mail.
- Não deve retornar senha ou hash.
- Não deve alterar estado nem criar evento.

### Login

`POST /api/v1/auth/login`

- Command: `LoginAccessUser` ou caso de uso de autenticacao com leitura de credenciais.
- Deve localizar o usuário por e-mail normalizado e verificar o hash da senha.
- Deve rejeitar credenciais inválidas com resposta genérica, sem revelar se o e-mail existe.
- Deve rejeitar usuário `disabled`.
- Em sucesso, deve retornar uma credencial de sessão ou token conforme decisão arquitetural registrada.
- Não deve registrar senha, token ou credencial em logs.
- Tentativas de login e eventos de segurança devem seguir a política de outbox quando houver consumidores de integração.

## CQRS E Outbox

Commands alteram estado no MySQL e queries somente leem o read model no Elasticsearch. Queries nunca devem consultar MySQL como fallback. A criação, alteração, desativação e eventos de segurança devem usar uma unidade transacional. A escrita do usuário e o registro do evento na outbox devem ser confirmados atomicamente.

O processador da outbox deve publicar eventos com entrega at-least-once, retry com backoff e comportamento idempotente. O algoritmo deve seguir: selecionar eventos `pending` e `available_at <= now`, tentar publicar no RabbitMQ, registrar tentativa e, em caso de erro, aumentar `attempts`, calcular delay exponencial e marcar `last_error`. Eventos com sucesso devem ser marcados como `published` e manter idempotência pelo `event_id` para evitar duplicação em reprocessamento.

O projetor RabbitMQ -> Elasticsearch deve consumir os eventos publicados pelo aggregate de usuário e aplicar as alterações no read model, preservando `id`, `email`, `name`, `status`, `version` e timestamps. A indexação deve ser idempotente por `id` do usuário e nunca depender de consultas ao MySQL. Consumidores devem aceitar duplicatas usando o identificador do evento.

A reindexação do Elasticsearch a partir do MySQL é uma operação de infraestrutura e manutenção, não parte do caminho de leitura normal da API. Quando o read model precisar ser reconstruído, um job de reindexação consulta o MySQL em batch, reescreve os documentos no Elasticsearch e invalida ou substitui os índices relevantes. As queries da API continuam 100% no Elasticsearch e nunca consultam MySQL como fallback.

## Modulos E Contratos

Os componentes abaixo são requisitos de implementação e devem ser criados como projetos, namespaces e arquivos C# próprios. A organização combina Clean Architecture com Vertical Slice: os limites arquiteturais ficam nos projetos e cada feature agrupa endpoint, caso de uso, contratos e testes relacionados.

### Organizacao Por Agregado

Arquivos que pertencem diretamente ao agregado `AccessUser` devem ficar agrupados por feature e namespace dentro da camada responsável, evitando poluir a pasta com arquivos de outros agregados. A organização esperada inclui:

```text
src/
  Backend.Api/
    Endpoints/AccessUsers/AccessUserEndpointGroup.cs
    Endpoints/AccessUsers/CreateAccessUserEndpoint.cs
    Endpoints/AccessUsers/UpdateAccessUserEndpoint.cs
    Endpoints/AccessUsers/GetAccessUserEndpoint.cs
    Endpoints/AccessUsers/ListAccessUsersEndpoint.cs
    Endpoints/Authentication/LoginEndpoint.cs
  Backend.Application/
    AccessUsers/Commands/CreateAccessUser/CreateAccessUserCommand.cs
    AccessUsers/Commands/CreateAccessUser/CreateAccessUserHandler.cs
    AccessUsers/Queries/GetAccessUser/GetAccessUserQuery.cs
    AccessUsers/Queries/GetAccessUser/GetAccessUserHandler.cs
    Abstractions/Persistence/IAccessUserWriteRepository.cs
    Abstractions/Persistence/IAccessUserReadRepository.cs
    Abstractions/Messaging/IEventPublisher.cs
  Backend.Domain/
    AccessUsers/AccessUser.cs
    AccessUsers/AccessUserId.cs
    AccessUsers/AccessUserEmail.cs
    AccessUsers/AccessUserStatus.cs
    AccessUsers/AccessUserCreatedEvent.cs
    AccessUsers/AccessUserUpdatedEvent.cs
  Backend.Infrastructure/
    Persistence/MySql/MySqlAccessUserWriteRepository.cs
    Persistence/Elasticsearch/ElasticsearchAccessUserReadRepository.cs
    Messaging/RabbitMq/RabbitMqEventPublisher.cs
    Messaging/RabbitMq/RabbitMqOutboxProcessor.cs
    Security/Argon2PasswordHasher.cs
    Security/JwtTokenService.cs
  Backend.Contracts/
    AccessUsers/CreateAccessUserRequest.cs
    AccessUsers/AccessUserResponse.cs
```

O mesmo critério deve ser aplicado a novos agregados e componentes de domínio. `Outbox/OutboxRecord.cs` e `Outbox/Processor.cs` permanecem na camada transversal; o namespace `Outbox.AccessUser` concentra os eventos específicos desse agregado. Não devem ser criadas classes de agrupamento sem responsabilidade própria apenas para formar namespaces.

A implementação deve seguir a divisão em módulos para preservar baixo acoplamento e manter a regra do projeto:

- `Domain`: entidades, value objects, enums e regras internas. Ex.: `AccessUser`, `AccessUserStatus`, `EmailAddress`, `UserId`, `PasswordHash`.
- `Application/Commands`: comandos de escrita. Ex.: `CreateAccessUserCommand`, `UpdateAccessUserCommand`, `LoginAccessUserCommand`, cada um recebendo um DTO e retornando resultado tipado com erros de validação ou domínio.
- `Application/Queries`: consultas de leitura. Ex.: `GetAccessUserQuery`, `ListAccessUsersQuery`, sempre retornando views públicas sem efeitos colaterais.
- `Application/Abstractions`: portas para escrita, leitura, mensageria e segurança. `IAccessUserWriteRepository` define `InsertAsync`, `UpdateAsync`, `FindByIdAsync`, `FindByEmailAsync` e `SaveOutboxEventAsync`; `IAccessUserReadRepository` define busca por id, filtros e paginação no Elasticsearch.
- `Outbox`: entidade de evento transacional e `BackgroundService` com retry, backoff, idempotência e observabilidade.
- `Auth`: validação de senha, emissão e validação de token, e policies/middleware para autorização por papel.
- `Api/Endpoints`: Minimal APIs, grupos de rota e conversão entre HTTP e casos de uso, sem lógica de domínio embutida.

### Convencao De Arquivos C#

Cada classe, record, interface ou componente principal deve ficar em um arquivo PascalCase correspondente à sua responsabilidade. Na infraestrutura, por exemplo, `MySqlAccessUserRepository` fica em `Infrastructure/MySql/MySqlAccessUserRepository.cs`, `ElasticsearchAccessUserReadRepository` em `Infrastructure/Elasticsearch/ElasticsearchAccessUserReadRepository.cs`, `RabbitMqEventPublisher` em `Infrastructure/RabbitMq/RabbitMqEventPublisher.cs` e `RedisSessionStore` em `Infrastructure/Redis/RedisSessionStore.cs`.

Os nomes de métodos devem refletir a operação ou o evento de domínio que executam, com especificidade suficiente para não confundir responsabilidades. Usar convenções .NET como `CreateAsync`, `FindByIdAsync`, `PublishAsync` e `HandleAsync`, sempre com `CancellationToken` quando houver I/O.

Quando uma entidade ou componente crescer, cada responsabilidade de domínio própria deve ficar em uma classe ou arquivo C# próprio. Por exemplo, a implementação de `AccessUserCreatedEvent` fica em `Outbox/AccessUser/AccessUserCreatedEvent.cs`, enquanto `Outbox/OutboxRecord.cs` permanece reservado ao modelo `OutboxRecord` e seus estados.

Para o evento `AccessUserUpdated`, o mesmo padrão exige `AccessUserUpdatedEvent` em `Outbox/AccessUser/AccessUserUpdatedEvent.cs`. Eventos diferentes devem possuir payloads de domínio diferentes, mesmo quando compartilham os mesmos campos, e a outbox deve aceitar ambos sem alterar o contrato JSON dos payloads.

`src/Backend.Api/Program.cs` fica fora desses limites como composition root da aplicação. Ele pode registrar rotas, middleware e dependências no IoC, mas não pode implementar validação, hash, regras de usuário, persistência, outbox ou armazenamento de estado. O endpoint de criação somente será considerado implementado quando respeitar o encadeamento `Endpoint -> Application/Commands -> Domain/Port -> Infrastructure/Adapter`.

Os contratos devem manter um boundary claro: endpoints transformam HTTP em commands/queries, handlers de aplicação executam validação e domínio, e adaptadores de infraestrutura são a única troca de dados com MySQL/Elasticsearch/Redis.

## Modelo De Usuario E Status

O modelo de domínio do usuário de acesso deve seguir a estrutura abaixo:

- `id`: identificador UUIDv7 ou bigint gerado por banco, imutavel;
- `email`: string normalizada para lowercase, sem espaços e validada por formato canonical;
- `name`: nome de exibição com limite de caracteres e validação de tamanho;
- `password_hash`: hash Argon2id, nunca armazenado em texto puro;
- `status`: enum `active | disabled`;
- `created_at` e `updated_at`: timestamps de auditoria;
- `version`: numero de versao para concorrencia otimista.

A tabela MySQL `access_users` deve seguir o esquema:

```sql
CREATE TABLE access_users (
  id BINARY(16) PRIMARY KEY,
  email VARCHAR(254) NOT NULL,
  name VARCHAR(255) NOT NULL,
  password_hash VARCHAR(255) NOT NULL,
  status ENUM('active', 'disabled') NOT NULL DEFAULT 'active',
  version BIGINT NOT NULL DEFAULT 0,
  created_at DATETIME(6) NOT NULL,
  updated_at DATETIME(6) NOT NULL,
  UNIQUE KEY uk_access_users_email (email)
) ENGINE=InnoDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

A tabela MySQL `access_users_outbox` deve registrar cada evento gerado pela escrita do usuário na mesma transação:

```sql
CREATE TABLE access_users_outbox (
  id BINARY(16) PRIMARY KEY,
  aggregate_id BINARY(16) NOT NULL,
  event_type VARCHAR(100) NOT NULL,
  payload JSON NOT NULL,
  status ENUM('pending', 'published', 'failed') NOT NULL DEFAULT 'pending',
  attempts INT NOT NULL DEFAULT 0,
  available_at DATETIME(6) NOT NULL,
  created_at DATETIME(6) NOT NULL,
  published_at DATETIME(6) NULL,
  last_error VARCHAR(1000) NULL,
  KEY ix_access_users_outbox_status_available (status, available_at)
) ENGINE=InnoDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

Regras de domínio:

- O e-mail é único no conjunto de usuários ativos e inativos e deve ser consultado em forma normalizada.
- O status `disabled` bloqueia login e desabilita operações de autenticação, mas não remove o registro.
- `PATCH` de usuário deve atualizar `updated_at` e incrementar `version` somente quando houver mudança relevante.
- Operações de escrita devem rejeitar condições de concorrência quando `version` informado pelo cliente divergir do registro em banco.

## Unidade Transacional De Usuario E Evento

A alteração do aggregate `AccessUser` e o registro do evento na outbox devem acontecer dentro da mesma transação InnoDB. O contrato de integração deve ser:

```text
begin transaction
  insert/update access_users
  insert access_users_outbox
commit
```

Se qualquer etapa falhar, a transação inteira deve ser revertida. Em termos de desenho da aplicação:

- o command handler valida a entrada e cria o evento de domínio;
- o repository de escrita persiste a entidade e o registro da outbox na mesma unidade transacional;
- o processador da outbox somente publica eventos após o commit bem-sucedido;
- a API não publica eventos fora da transação, nem grava eventos de domínio em handlers HTTP.

Isso garante consistência entre estado e integração, sem permitir que o usuário seja alterado sem que um evento validado tenha sido registrado.

## Seguranca

- Nunca armazenar ou retornar senha em texto puro.
- Usar biblioteca de hash de senha revisada, com parâmetros configuráveis.
- Aplicar validação de entrada e limites de tamanho.
- Evitar enumeração de usuários no login.
- Definir autenticação e autorização para operações administrativas antes de liberar a API em produção.
- Definir política de rate limiting e bloqueio de tentativas antes de expor o login publicamente.

## Decisoes Registradas

- Banco de dados e provider de persistência: a decisão arquitetural permanece em `MySQL/InnoDB` com `utf8mb4` e repositório dedicado em C#/.NET, conforme ADR da infraestrutura.
- Formato da credencial: usar JWT de acesso stateless com `sub`, `role` e `exp`, assinado com `RS256`. O token de acesso expira em 15 minutos e um refresh token, quando existir, é armazenado no Redis com TTL para revogação e invalidação rápida.
- Regras de autorização: `admin` pode criar, alterar, listar e desabilitar qualquer usuário; `user` pode consultar o próprio perfil e atualizar apenas dados não sensíveis e de sua própria conta; anônimos não podem acessar endpoints de gestão. Toda operação valida o papel do token em cada request.
- Algoritmo de hash: usar `Argon2id` com parâmetros configuráveis (memória 64 MiB, time cost 3, parallelism 2), armazenando somente `password_hash` e nunca a senha em texto puro. A política de rotação de senha exige rehash ao detectar parâmetros antigos ou quando a senha for alterada.
- Estratégia de rate limiting e bloqueio de tentativas: aplicar limite por IP e por e-mail para login, com backoff exponencial e bloqueio temporário em Redis; respostas do login devem continuar genéricas para evitar enumeração.

## Testes Obrigatorios

Os testes devem ser mantidos nos projetos `Backend.Api.UnitTests` e `Backend.Api.IntegrationTests`, segmentados por responsabilidade. A suíte não deve ficar concentrada em uma classe monolítica.

Estrutura esperada:

```text
tests/
  Backend.Api.UnitTests/
  Backend.Api.IntegrationTests/
```

- Testes unitários de validação, normalização, senha, transições de status e regras de domínio.
- Testes de command handlers para duplicidade, concorrencia e atomicidade com a outbox.
- Testes de queries sem efeitos colaterais e sem exposicao de hash.
- Testes de login bem-sucedido, senha inválida, usuário inexistente e usuário desabilitado.
- Testes HTTP dos contratos, codigos de status e formato das respostas.
- Testes do processador da outbox para retry, idempotência e falha de publicação.
