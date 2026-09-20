# Tarefas: Gestão De Usuário De Acesso

## Decisões E Fundação

- [x] Registrar MySQL/InnoDB, `utf8mb4` e estratégia de migração.
- [x] Escolher e registrar o provider ADO.NET/EF Core ou micro-ORM para persistência MySQL; manter a escolha do pacote RabbitMQ pendente até a task do publisher.
- [x] Registrar formato da credencial, algoritmo de hash e política de autorização.
- [x] Registrar RabbitMQ como broker e Redis para sessões e dados temporários.
- [x] Registrar Elasticsearch como read model exclusivo das queries.
- [x] Registrar uma tabela de outbox por tabela de domínio que produza eventos.
- [x] Definir projetos, namespaces e contratos de commands, queries, domínio e portas de infraestrutura.
- [x] Definir Clean Architecture + Vertical Slice, SOLID pragmático e IoC por `IServiceCollection`.
- [x] Definir endpoints Minimal API versionados em `/api/v1` com `ProblemDetails` e OpenAPI.
- [x] Definir modelo de usuário, status, unicidade de e-mail e concorrência otimista.

## Persistência E Outbox

- [x] Criar migração/tabela de usuários sem armazenar senha em texto puro.
- [x] Criar tabela transacional de outbox por tabela de domínio, com status, tentativas e timestamps.
- [x] Implementar unidade transacional real no MySQL/InnoDB para usuário e evento da outbox.
- [x] Documentar e configurar stack Docker Compose para subir infraestrutura e aplicar migrações SQL no bootstrap.
- [x] Documentar processador da outbox com retry, backoff e idempotência.
- [ ] Implementar processador da outbox com retry, backoff e idempotência usando RabbitMQ real.
- [x] Documentar projetor RabbitMQ -> Elasticsearch.
- [ ] Implementar projetor RabbitMQ -> Elasticsearch usando consumidor RabbitMQ real.
- [x] Documentar reindexação do Elasticsearch a partir do MySQL sem usar MySQL no caminho normal das queries.
- [ ] Implementar reindexação do Elasticsearch a partir do MySQL sem usar MySQL no caminho normal das queries.

## Endpoints E Casos De Uso

- [x] Criar a estrutura de projetos prevista em `docs/plans/initial-architecture.md`: `Backend.Api`, `Backend.Application`, `Backend.Domain`, `Backend.Infrastructure` e `Backend.Contracts`.
- [x] Implementar `POST /api/v1/access-users` seguindo `Endpoint -> Command -> domínio -> repositório MySQL/outbox`, com validação de e-mail, nome e senha; e-mail único, hash Argon2id, evento transacional na outbox e resposta sem `password_hash`.
- [ ] Implementar `PATCH /api/v1/access-users/{id}`.
- [ ] Implementar `GET /api/v1/access-users/{id}`.
- [ ] Implementar `GET /api/v1/access-users` com paginação.
- [ ] Implementar `POST /api/v1/auth/login`.
- [ ] Garantir que respostas e logs nunca exponham senha, hash ou token.

## Testes E Retirada

- [x] Verificar que `src/Backend.Api/Program.cs` contém somente composition root, middleware, DI e rotas, sem regras de domínio, hash, persistência ou estado de usuários.
- [x] Criar os projetos de testes `Backend.Api.UnitTests` e `Backend.Api.IntegrationTests`, segmentados por camada.
- [x] Remover a concentração da suíte em uma classe monolítica de testes.
- [ ] Adicionar testes de domínio em `Backend.Api.UnitTests/Domain/` para validação, normalização, senha e transições de status.
- [ ] Adicionar testes de commands e queries em `Backend.Api.UnitTests/Application/`, cobrindo concorrência e regras sem efeitos colaterais.
- [ ] Adicionar testes de repositórios em `Backend.Api.IntegrationTests/Repositories/`, incluindo unicidade e atomicidade com a outbox em MySQL real.
- [ ] Adicionar testes do processador em `Backend.Api.IntegrationTests/Outbox/`, cobrindo retry, backoff e reprocessamento após falha de publicação em RabbitMQ real.
- [ ] Adicionar testes HTTP em `Backend.Api.IntegrationTests/Http/`, usando `WebApplicationFactory` e cobrindo endpoints, códigos de erro, login válido, inválido, inexistente e usuário desabilitado.
- [ ] Adicionar testes de queries por id, filtros, paginação e busca no Elasticsearch.
- [ ] Adicionar teste de falha do Elasticsearch sem fallback para MySQL.
- [ ] Executar os testes de persistência, outbox e endpoints contra MySQL, RabbitMQ, Elasticsearch e Redis reais via Docker Compose, incluindo `dotnet format --verify-no-changes`, `dotnet test` e `dotnet build --warnaserror`.
- [x] Remover rotas e testes das funcionalidades legadas de bootstrap após a nova API estar validada.
- [ ] Atualizar a especificação, o plano e esta lista com os resultados.
- [ ] Publicar OpenAPI dos endpoints e validar respostas de erro com `ProblemDetails`.

## Gate De Conclusão

Nenhuma tarefa de endpoint pode ser marcada como concluída enquanto a estrutura modular prevista não existir, enquanto `Program.cs` concentrar responsabilidades de domínio, aplicação ou infraestrutura ou enquanto o runtime usar `InMemoryAccessUserRepository`. A revisão deve conferir os caminhos dos arquivos, os contratos entre camadas, a conexão com MySQL/RabbitMQ e os testes correspondentes.

Uma implementação só pode ser marcada como concluída quando usa a infraestrutura final do ambiente: MySQL/InnoDB para o write model, outbox na mesma transação e RabbitMQ para a publicação posterior ao commit. Doubles em memória são permitidos somente em testes unitários isolados e não podem ser registrados como implementação do endpoint.
