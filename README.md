# REST API .NET

API backend completa em C# com ASP.NET Core sobre .NET 10 LTS. Gestão de usuários de acesso é uma das features planejadas do backend.

## Arquitetura

A solução usa Clean Architecture + Vertical Slice em .NET 10 LTS. Os projetos são `Backend.Api`, `Backend.Application`, `Backend.Domain`, `Backend.Infrastructure` e `Backend.Contracts`; `Program.cs` é o composition root e as dependências são registradas por IoC via `IServiceCollection`.

A API usa Minimal APIs, endpoints organizados por feature, PascalCase para arquivos/classes, versionamento `/api/v1`, OpenAPI e `ProblemDetails` para respostas de erro.

- **CQRS**: commands alteram o write model; queries consultam o read model.
- **MySQL 9.7.2 / InnoDB**: write model e fonte de verdade.
- **Transactional Outbox**: uma outbox própria para cada tabela de domínio que produzir eventos.
- **RabbitMQ 4.3.6**: transporte de eventos após o commit.
- **Elasticsearch 9.5.4**: read model exclusivo das queries, inclusive consultas por id.
- **Redis 8.8**: somente sessões, tokens revogados, rate limiting e dados temporários; não participa das queries de usuários.
- **.NET 10 LTS**: runtime e SDK padrao da aplicacao.
- **Docker**: ambiente padrão para restore, build, testes e execução.

O fluxo de desenvolvimento é:

```text
spec -> plan -> tasks -> implementation -> tests -> update docs
```

## Executar Com Docker

A stack completa do backend deve subir via Docker Compose, incluindo os serviços de banco, fila, busca, cache, configurações de bootstrap e os scripts de inicialização necessários para que o ambiente local fique funcional desde o primeiro `up`.

Subir toda a stack de desenvolvimento:

```bash
docker compose up --build
```

Subir apenas o servico HTTP atual:

```bash
docker compose up --build rest-server
```

Executar os testes no estagio Docker de testes:

```bash
docker compose --profile test build rest-test
docker compose --profile test run --rm rest-test
```

O servico `rest-test` executa a suite .NET isoladamente e nao inicia MySQL, RabbitMQ, Elasticsearch ou Redis. Os testes HTTP substituem as portas externas por doubles; os testes de infraestrutura devem usar o Compose completo.

A API fica disponivel em `https://localhost:8443` usando o certificado de desenvolvimento do ASP.NET Core. O endpoint HTTP `http://localhost:8080` permanece disponivel para redirecionar clientes para HTTPS quando executado com `dotnet run`.

### HTTPS Local

Confiar no certificado de desenvolvimento uma vez no host:

```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

Executar a API localmente:

```bash
dotnet run --project src/Backend.Api/Backend.Api.csproj
```

Para executar pelo Docker Compose, exportar o certificado para o caminho montado pelo serviço:

```bash
mkdir -p "$HOME/.aspnet/https"
dotnet dev-certs https -ep "$HOME/.aspnet/https/backend-api.pfx" -p local-development-only
docker compose up --build rest-server
```

O Compose publica `https://localhost:8443` e usa o certificado apenas dentro do ambiente local. Como o certificado de desenvolvimento não é uma autoridade pública, clientes de linha de comando podem precisar de `curl -k` até que a cadeia local seja confiada.

### Bootstrap no Docker Compose

- As migracoes SQL do MySQL devem ficar em `migrations/*.sql` e ser versionadas no repositorio.
- O ambiente Docker Compose deve incluir inicializacao automatica para MySQL, RabbitMQ e Elasticsearch antes da API ficar pronta para uso.
- O bootstrap do RabbitMQ deve criar exchange, fila e bindings basicos para o fluxo da outbox.
- O bootstrap do Elasticsearch deve criar indices e mappings iniciais, como `access_users`, sem depender do app para criar o schema em runtime.
- O bootstrap da stack deve garantir que `access_users`, `access_users_outbox`, fila de eventos e indice de consulta sejam criados automaticamente ao subir a infraestrutura.
- A aplicacao nao deve depender de SQL gerado em runtime em handlers HTTP; a schema deve ser aplicada por migracao reproducivel.

## Desenvolvimento Orientado Por Especificacao

Antes de implementar uma funcionalidade:

1. Atualize ou crie a especificacao em `docs/specs/`.
2. Registre decisoes tecnicas em `docs/decisions/`.
3. Atualize o plano em `docs/plans/`.
4. Derive tarefas em `docs/tasks/`.
5. Implemente em modulos separados por responsabilidade.
6. Adicione testes unitarios e de integracao.
7. Atualize a documentacao e execute as validacoes Docker.

## Documentacao

- [`AGENTS.md`](AGENTS.md): instrucoes compartilhadas para pessoas e agentes de qualquer ferramenta.
- [`docs/README.md`](docs/README.md): indice da documentacao SDD.
- [`docs/constitution.md`](docs/constitution.md): principios e Definition of Done.
- [`docs/architecture.md`](docs/architecture.md): arquitetura e limites entre write/read model.
- [`docs/specs/access-user-management.md`](docs/specs/access-user-management.md): especificacao da API de usuarios.
- [`docs/infrastructure/`](docs/infrastructure/): MySQL, RabbitMQ, Elasticsearch e Redis.
- [`docs/decisions/`](docs/decisions/): ADRs e consequencias das escolhas.
- [`docs/plans/`](docs/plans/): planos de implementacao.
- [`docs/tasks/`](docs/tasks/): checklists executaveis.

## Regras Para Desenvolvimento

- Manter commands, queries, dominio, persistencia, handlers e projetores em modulos separados.
- Nao consultar MySQL no caminho normal das queries; usar Elasticsearch.
- Nao publicar eventos antes do commit da transacao.
- Nunca armazenar ou expor senhas em texto puro.
- Criar testes para cada comportamento novo ou alterado.
- Revisar nullability, concorrencia, ciclos de vida do DI, cancelamento e possiveis vazamentos de recursos.
