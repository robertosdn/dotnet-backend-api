# Plano: Evolução Do Backend
## Objetivo
Implementar as primeiras features reais do backend, iniciando pela gestão de usuários de acesso e mantendo CQRS, Transactional Outbox, segurança de credenciais e testes automatizados.
## Contratos E Projetos Previstos
Esta estrutura é obrigatória para a implementação, não apenas uma sugestão de organização. Cada responsabilidade deve existir no projeto indicado antes de a etapa correspondente ser marcada como concluída. `src/Backend.Api/Program.cs` deve permanecer limitado à composição do host, DI e registro de rotas; ele não pode conter entidades, value objects, regras de validação, hash de senha, acesso a repositórios, armazenamento de estado ou orquestração de commands.
A estrutura inicial do backend deve seguir a separação abaixo, mantendo CQRS e a outbox transacional:
### Contratos esperados
### Regra de implementação
Para cada endpoint novo, a implementação deve seguir o fluxo `Endpoint -> Command/Query -> Handler -> Domain/Port -> Adapter`, com contratos definidos em arquivos próprios. O endpoint somente desserializa a entrada, chama o handler e converte o resultado em resposta HTTP. Uma implementação não pode ser aceita se regras de domínio ou persistência estiverem concentradas em `Program.cs` ou em um único arquivo monolítico.
Os testes também devem seguir a separação modular. Os projetos `Backend.Api.UnitTests` e `Backend.Api.IntegrationTests` são obrigatórios para a suíte principal, com classes separadas para domínio, commands/queries, repositórios, outbox e HTTP. Uma classe única de testes não atende ao plano. Testes unitários devem usar xUnit e testes HTTP devem usar `WebApplicationFactory` quando necessários.
## Etapas
1. Fechar as decisões em aberto da especificação de gestão de usuários, incluindo banco, credencial, hash, autorização e destino de eventos.
2. Escolher banco de dados e pacote/provider de acesso .NET, documentando a decisão arquitetural.
3. Criar os projetos `Backend.Api`, `Backend.Application`, `Backend.Domain`, `Backend.Infrastructure` e `Backend.Contracts`, com dependências apontando para dentro.
4. Criar o modelo da outbox e a unidade transacional que grava usuário e evento atomicamente.
5. Implementar criação, alteração e consultas de usuários com MySQL/Elasticsearch reais e testes unitários e de integração segmentados nos projetos `*.Tests` por camada.
6. Implementar login com verificação segura de senha, credencial de sessão/token e respostas que evitem enumeração.
7. Implementar o processador da outbox com retry, backoff, idempotência e observabilidade, com seleção de eventos pendentes, tentativa de publicação no RabbitMQ, marcação de erro e reprocessamento controlado.
8. Implementar o projetor RabbitMQ -> Elasticsearch para materializar o read model dos usuários sem consultas ao MySQL em queries normais.
9. Implementar reindexação de emergência do Elasticsearch a partir do MySQL em batch, mantendo o caminho normal das queries somente no Elasticsearch.
10. Atualizar Docker Compose com banco e demais dependências necessárias, aplicar migrações SQL em bootstrap e validar os endpoints contra a infraestrutura real pelo perfil de testes do .NET.
11. Consolidar o router somente com endpoints de negócio após a cobertura dos casos de uso reais.
Em cada etapa de implementação, a revisão deve verificar a árvore de arquivos, os limites de dependência entre projetos/namespaces, a infraestrutura efetivamente usada pelo runtime e a existência de testes da camada alterada nos projetos `*.Tests`. A tarefa só pode ser marcada como concluída quando essa verificação passar; mocks em memória não substituem MySQL, RabbitMQ ou Elasticsearch nos fluxos de integração.
## Não Escopo
- Não implementar autenticação administrativa sem definir autorização mínima.
- Não escolher banco, formato de credencial ou broker sem uma decisão registrada.
- Não publicar eventos diretamente a partir de handlers HTTP.
## Critério De Saída
# Plano: Evolucao Do Backend

## Objetivo

Implementar as primeiras features reais do backend, iniciando pela gestao de usuarios de acesso e mantendo CQRS, Transactional Outbox, seguranca de credenciais e testes automatizados.

## Contratos E Projetos Previstos

Esta estrutura e obrigatoria para a implementacao, nao apenas uma sugestao de organizacao. Cada responsabilidade deve existir no projeto indicado antes de a etapa correspondente ser marcada como concluida. `src/Backend.Api/Program.cs` deve permanecer limitado a composicao do host, DI e registro de rotas; ele nao pode conter entidades, value objects, regras de validacao, hash de senha, acesso a repositorios, armazenamento de estado ou orquestracao de commands.

A estrutura inicial do backend deve seguir a separacao abaixo, mantendo CQRS e a outbox transacional:

```text
Backend.sln
src/
  Backend.Api/
    Program.cs
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
    DependencyInjection/PersistenceServiceCollectionExtensions.cs
    DependencyInjection/MessagingServiceCollectionExtensions.cs
    DependencyInjection/SecurityServiceCollectionExtensions.cs
  Backend.Contracts/
    AccessUsers/CreateAccessUserRequest.cs
    AccessUsers/AccessUserResponse.cs
tests/
  Backend.Domain.Tests/
  Backend.Application.Tests/
  Backend.Infrastructure.IntegrationTests/
  Backend.Api.IntegrationTests/
```

### Contratos esperados

- `AccessUser` e `AccessUserStatus` ficam no projeto de dominio e encapsulam regras de email, nome, senha e transicoes de status.
- Commands aceitam requests validos e retornam resultados tipados ou erros de aplicacao mapeados para `ProblemDetails`.
- Queries retornam views publicas do read model, sem efeitos colaterais e sem acesso ao write model.
- Repositórios de escrita expõem `insert`, `update`, `find_by_id`, `find_by_email`, `save_event` e operam dentro de unidade transacional com outbox.
- A tabela `access_users_outbox` registra os eventos do aggregate com `id`, `aggregate_id`, `event_type`, `payload`, `status`, `attempts`, `available_at`, `created_at`, `published_at` e `last_error`, em uma transacao compartilhada com o usuario.
- Repositórios de leitura consultam o Elasticsearch e nunca fazem fallback para MySQL em consultas normais.
- Autenticacao e autorizacao sao componentes separados: login valida senha e emissao de token; policies/middleware validam `sub`, `role` e escopos por request.

### Regra de implementacao

Para cada endpoint novo, a implementacao deve seguir o fluxo `Endpoint -> Command/Query -> Handler -> Domain/Port -> Adapter`, com contratos definidos em arquivos proprios. O endpoint somente desserializa a entrada, chama o handler e converte o resultado em resposta HTTP. Uma implementacao nao pode ser aceita se regras de dominio ou persistencia estiverem concentradas em `Program.cs` ou em um unico arquivo monolitico.

Os testes tambem devem seguir a separacao modular. Os projetos `Backend.Api.UnitTests` e `Backend.Api.IntegrationTests` sao obrigatorios para a suite principal, com classes separadas para dominio, commands/queries, repositorios, outbox e HTTP. Uma classe unica de testes nao atende ao plano. Testes unitarios devem usar xUnit e testes HTTP devem usar `WebApplicationFactory` quando necessarios.

## Etapas

1. Fechar as decisoes em aberto da especificacao de gestao de usuarios, incluindo banco, credencial, hash, autorizacao e destino de eventos.
2. Escolher banco de dados e pacote/provider de acesso .NET, documentando a decisao arquitetural.
3. Criar os projetos `Backend.Api`, `Backend.Application`, `Backend.Domain`, `Backend.Infrastructure` e `Backend.Contracts`, com dependencias apontando para dentro.
4. Criar o modelo da outbox e a unidade transacional que grava usuario e evento atomicamente.
5. Implementar criacao, alteracao e consultas de usuarios com MySQL/Elasticsearch reais e testes unitarios e de integracao segmentados nos projetos `*.Tests` por camada.
6. Implementar login com verificacao segura de senha, credencial de sessao/token e respostas que evitem enumeracao.
7. Implementar o processador da outbox com retry, backoff, idempotencia e observabilidade, com selecao de eventos pendentes, tentativa de publicacao no RabbitMQ, marcacao de erro e reprocessamento controlado.
8. Implementar o projetor RabbitMQ -> Elasticsearch para materializar o read model dos usuarios sem consultas ao MySQL em queries normais.
9. Implementar reindexacao de emergencia do Elasticsearch a partir do MySQL em batch, mantendo o caminho normal das queries somente no Elasticsearch.
10. Atualizar Docker Compose com banco e demais dependencias necessarias, aplicar migracoes SQL em bootstrap e validar os endpoints contra a infraestrutura real pelo perfil de testes do .NET.
11. Consolidar o router somente com endpoints de negocio apos a cobertura dos casos de uso reais.

Em cada etapa de implementacao, a revisao deve verificar a arvore de arquivos, os limites de dependencia entre projetos/namespaces, a infraestrutura efetivamente usada pelo runtime e a existencia de testes da camada alterada nos projetos `*.Tests`. A tarefa so pode ser marcada como concluida quando essa verificacao passar; mocks em memoria nao substituem MySQL, RabbitMQ ou Elasticsearch nos fluxos de integracao.

## Nao Escopo

- Nao implementar autenticacao administrativa sem definir autorizacao minima.
- Nao escolher banco, formato de credencial ou broker sem uma decisao registrada.
- Nao publicar eventos diretamente a partir de handlers HTTP.

## Criterio De Saida

A primeira feature deve permitir criar, alterar, consultar e autenticar usuarios de acesso, demonstrar que a alteracao de estado e o evento da outbox sao confirmados juntos e remover os endpoints de demonstracao sem reduzir a cobertura de testes. O backend deve permanecer preparado para novas features de negocio.

O criterio de saida inclui a estrutura modular prevista: dominio, commands, queries, repositorios, outbox, autenticacao e HTTP devem estar separados em projetos/namespaces proprios, com `Program.cs` contendo somente a montagem do host, DI e rotas.

Tambem inclui uma suite de testes segmentada nos projetos `*.Tests`, sem concentrar os testes em uma classe monolitica.
