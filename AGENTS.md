# Instrucoes Do Projeto

Estas instrucoes sao aplicaveis a pessoas e a qualquer agente de desenvolvimento utilizado neste repositorio.

## Objetivo

Desenvolver uma API backend completa em .NET 10 LTS com C#, modular, testavel e preparada para evoluir com multiplas features de negocio, incluindo gestao de usuarios de acesso.

## Arquitetura Obrigatoria

- Usar CQRS: commands alteram o write model; queries somente consultam o read model.
- Usar MySQL com InnoDB e `utf8mb4` como write model e fonte de verdade.
- Usar Transactional Outbox: cada tabela de dominio que produzir eventos deve ter sua propria outbox, gravada atomicamente com a alteracao de dominio.
- Usar RabbitMQ para transportar eventos apos o commit.
- Usar Elasticsearch como read model exclusivo das queries, inclusive consultas por id. Queries nao devem consultar MySQL como fallback.
- Usar Redis somente para sessoes, tokens revogados, rate limiting e dados temporarios. Redis nao participa do read model de usuarios.

## Modularidade

- Usar Clean Architecture + Vertical Slice: separar `Backend.Api`, `Backend.Application`, `Backend.Domain`, `Backend.Infrastructure` e `Backend.Contracts`.
- Separar dominio, commands, queries, endpoints HTTP, persistencia, outbox, projetores e infraestrutura em classes e arquivos PascalCase proprios.
- Aplicar SOLID de forma pragmatica e usar IoC pelo `IServiceCollection`; `Program.cs` deve ser somente o composition root.
- Manter metodos pequenos, responsabilidades claras e componentes reutilizaveis.
- Evitar arquivos monoliticos e abstrações artificiais.

## Seguranca E Memoria

- Nunca armazenar ou expor senhas em texto puro.
- Usar nullable reference types, `Result`/`ProblemDetails`, `async`/`await`, `CancellationToken` e `IDisposable`/`IAsyncDisposable` para tratar estados e recursos.
- Revisar concorrencia, tasks, canais, locks, conexoes, cancellation tokens e escopos de DI.
- Procurar nullability incorreta, deadlocks, data races logicas, vazamentos de conexoes e tarefas nao observadas.

## Docker E Validacao

- Usar Docker Compose ou as etapas do Dockerfile para restaurar, compilar, testar e executar .NET.
- Executar as validacoes aplicaveis dentro do Docker, incluindo `dotnet format --verify-no-changes`, `dotnet test` e `dotnet build --warnaserror`.
- Nao considerar uma alteracao concluida sem relatar os comandos executados e seus resultados.

## SDD E Sincronizacao

O padrao de desenvolvimento Specification-Driven Development (SDD) deste projeto esta documentado em [`docs/README.md`](docs/README.md). Consulte a documentacao referenciada ali antes de implementar uma funcionalidade.

Seguir o fluxo:

```text
spec -> plan -> tasks -> implementation -> tests -> update docs
```

Sempre que alterar infraestrutura, dependencias, `Dockerfile`, `docker-compose.yml`, variaveis de ambiente ou comandos de build, teste e execucao:

- Atualizar os arquivos `.md` correspondentes em `README.md`, `docs/`, planos, tarefas e ADRs aplicaveis.
- Atualizar todos os exemplos de comandos afetados.
- Atualizar health checks, portas, volumes e configuracoes documentadas quando mudarem.
- Manter codigo, configuracao operacional, comandos e documentacao sincronizados.

## Mudancas

- Ler a especificacao, o plano, as tarefas e os testes relacionados antes de editar.
- Fazer a menor mudanca coerente com a arquitetura existente.
- Adicionar ou atualizar testes unitarios e de integracao para cada comportamento alterado.
- Nao descartar mudancas preexistentes nem alterar arquivos fora do escopo sem necessidade.
