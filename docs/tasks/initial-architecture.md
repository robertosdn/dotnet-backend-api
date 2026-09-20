# Tarefas: Gestao De Usuario De Acesso

## Decisoes E Fundacao

- [x] Registrar MySQL/InnoDB, `utf8mb4` e estrategia de migracao.
- [x] Escolher e registrar o provider ADO.NET/EF Core ou micro-ORM para persistencia MySQL; manter a escolha do pacote RabbitMQ pendente ate a task do publisher.
- [x] Registrar formato da credencial, algoritmo de hash e politica de autorizacao.
- [x] Registrar RabbitMQ como broker e Redis para sessoes e dados temporarios.
- [x] Registrar Elasticsearch como read model exclusivo das queries.
- [x] Registrar uma tabela de outbox por tabela de dominio que produza eventos.
- [x] Definir projetos, namespaces e contratos de commands, queries, dominio e portas de infraestrutura.
- [x] Definir Clean Architecture + Vertical Slice, SOLID pragmatico e IoC por `IServiceCollection`.
- [x] Definir endpoints Minimal API versionados em `/api/v1` com `ProblemDetails` e OpenAPI.
- [x] Definir modelo de usuario, status, unicidade de email e concorrencia otimista.

## Persistencia E Outbox

- [x] Criar migracao/tabela de usuarios sem armazenar senha em texto puro.
- [x] Criar tabela transacional de outbox por tabela de dominio, com status, tentativas e timestamps.
- [x] Implementar unidade transacional real no MySQL/InnoDB para usuario e evento da outbox.
- [x] Documentar e configurar stack Docker Compose para subir infraestrutura e aplicar migrações SQL no bootstrap.
- [x] Documentar processador da outbox com retry, backoff e idempotencia.
- [ ] Implementar processador da outbox com retry, backoff e idempotencia usando RabbitMQ real.
- [x] Documentar projetor RabbitMQ -> Elasticsearch.
- [ ] Implementar projetor RabbitMQ -> Elasticsearch usando consumidor RabbitMQ real.
- [x] Documentar reindexacao do Elasticsearch a partir do MySQL sem usar MySQL no caminho normal das queries.
- [ ] Implementar reindexacao do Elasticsearch a partir do MySQL sem usar MySQL no caminho normal das queries.

## Endpoints E Casos De Uso

- [x] Criar a estrutura de projetos prevista em `docs/plans/initial-architecture.md`: `Backend.Api`, `Backend.Application`, `Backend.Domain`, `Backend.Infrastructure` e `Backend.Contracts`.
- [x] Implementar `POST /api/v1/access-users` seguindo `Endpoint -> Command -> dominio -> repositorio MySQL/outbox`, com validacao de email, nome e senha; email unico, hash Argon2id, evento transacional na outbox e resposta sem `password_hash`.
- [ ] Implementar `PATCH /api/v1/access-users/{id}`.
- [ ] Implementar `GET /api/v1/access-users/{id}`.
- [ ] Implementar `GET /api/v1/access-users` com paginacao.
- [ ] Implementar `POST /api/v1/auth/login`.
- [ ] Garantir que respostas e logs nunca exponham senha, hash ou token.

## Testes E Retirada

- [x] Verificar que `src/Backend.Api/Program.cs` contem somente composition root, middleware, DI e rotas, sem regras de dominio, hash, persistencia ou estado de usuarios.
- [x] Criar os projetos de testes `Backend.Api.UnitTests` e `Backend.Api.IntegrationTests`, segmentados por camada.
- [x] Remover a concentracao da suite em uma classe monolitica de testes.
- [ ] Adicionar testes de dominio em `Backend.Api.UnitTests/Domain/` para validacao, normalizacao, senha e transicoes de status.
- [ ] Adicionar testes de commands e queries em `Backend.Api.UnitTests/Application/`, cobrindo concorrencia e regras sem efeitos colaterais.
- [ ] Adicionar testes de repositorios em `Backend.Api.IntegrationTests/Repositories/`, incluindo unicidade e atomicidade com a outbox em MySQL real.
- [ ] Adicionar testes do processador em `Backend.Api.IntegrationTests/Outbox/`, cobrindo retry, backoff e reprocessamento apos falha de publicacao em RabbitMQ real.
- [ ] Adicionar testes HTTP em `Backend.Api.IntegrationTests/Http/`, usando `WebApplicationFactory` e cobrindo endpoints, codigos de erro, login valido, invalido, inexistente e usuario desabilitado.
- [ ] Adicionar testes de queries por id, filtros, paginacao e busca no Elasticsearch.
- [ ] Adicionar teste de falha do Elasticsearch sem fallback para MySQL.
- [ ] Executar os testes de persistencia, outbox e endpoints contra MySQL, RabbitMQ, Elasticsearch e Redis reais via Docker Compose, incluindo `dotnet format --verify-no-changes`, `dotnet test` e `dotnet build --warnaserror`.
- [x] Remover rotas e testes das funcionalidades legadas de bootstrap apos a nova API estar validada.
- [ ] Atualizar a especificacao, o plano e esta lista com os resultados.
- [ ] Publicar OpenAPI dos endpoints e validar respostas de erro com `ProblemDetails`.

## Gate De Conclusao

Nenhuma tarefa de endpoint pode ser marcada como concluida enquanto a estrutura modular prevista nao existir, enquanto `Program.cs` concentrar responsabilidades de dominio, aplicacao ou infraestrutura ou enquanto o runtime usar `InMemoryAccessUserRepository`. A revisao deve conferir os caminhos dos arquivos, os contratos entre camadas, a conexao com MySQL/RabbitMQ e os testes correspondentes.

Uma implementacao so pode ser marcada como concluida quando usa a infraestrutura final do ambiente: MySQL/InnoDB para o write model, outbox na mesma transacao e RabbitMQ para a publicacao posterior ao commit. Doubles em memoria sao permitidos somente em testes unitarios isolados e nao podem ser registrados como implementacao do endpoint.
