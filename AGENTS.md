# Instrucoes Do Projeto

Estas instrucoes sao aplicaveis a pessoas e a qualquer agente de desenvolvimento utilizado neste repositorio.

## Objetivo

Desenvolver uma API backend completa em .NET 10 LTS com C#, modular, testável e preparada para evoluir com múltiplas features de negócio, incluindo gestão de usuários de acesso.

## Arquitetura Obrigatoria

- Usar CQRS: commands alteram o write model; queries somente consultam o read model.
- Usar MySQL com InnoDB e `utf8mb4` como write model e fonte de verdade.
- Usar Transactional Outbox: cada tabela de domínio que produzir eventos deve ter sua própria outbox, gravada atomicamente com a alteração de domínio.
- Usar RabbitMQ para transportar eventos após o commit.
- Usar Elasticsearch como read model exclusivo das queries, inclusive consultas por id. Queries não devem consultar MySQL como fallback.
- Usar Redis somente para sessões, tokens revogados, rate limiting e dados temporários. Redis não participa do read model de usuários.

## Modularidade

- Usar Clean Architecture + Vertical Slice: separar `Backend.Api`, `Backend.Application`, `Backend.Domain`, `Backend.Infrastructure` e `Backend.Contracts`.
- Separar domínio, commands, queries, endpoints HTTP, persistência, outbox, projetores e infraestrutura em classes e arquivos PascalCase próprios.
- Aplicar SOLID de forma pragmática e usar IoC pelo `IServiceCollection`; `Program.cs` deve ser somente o composition root.
- Manter métodos pequenos, responsabilidades claras e componentes reutilizáveis.
- Evitar arquivos monolíticos e abstrações artificiais.

## Seguranca E Memoria

- Nunca armazenar ou expor senhas em texto puro.
- Usar nullable reference types, `Result`/`ProblemDetails`, `async`/`await`, `CancellationToken` e `IDisposable`/`IAsyncDisposable` para tratar estados e recursos.
- Revisar concorrência, tasks, canais, locks, conexões, cancellation tokens e escopos de DI.
- Procurar nullability incorreta, deadlocks, data races lógicas, vazamentos de conexões e tarefas não observadas.

## Docker E Validacao

- Usar Docker Compose ou as etapas do Dockerfile para restaurar, compilar, testar e executar .NET.
- Executar as validações aplicáveis dentro do Docker, incluindo `dotnet format --verify-no-changes`, `dotnet test` e `dotnet build --warnaserror`.
- Não considerar uma alteração concluída sem relatar os comandos executados e seus resultados.

## SDD E Sincronizacao

O padrao de desenvolvimento Specification-Driven Development (SDD) deste projeto esta documentado em [`docs/README.md`](docs/README.md). Consulte a documentacao referenciada ali antes de implementar uma funcionalidade.

Seguir o fluxo:

```text
spec -> plan -> tasks -> implementation -> tests -> update docs
```

Sempre que alterar infraestrutura, dependencias, `Dockerfile`, `docker-compose.yml`, variaveis de ambiente ou comandos de build, teste e execucao:

- Atualizar os arquivos `.md` correspondentes em `README.md`, `docs/`, planos, tarefas e ADRs aplicáveis.
- Atualizar todos os exemplos de comandos afetados.
- Atualizar health checks, portas, volumes e configurações documentadas quando mudarem.
- Manter código, configuração operacional, comandos e documentação sincronizados.

## Mudancas

- Ler a especificação, o plano, as tarefas e os testes relacionados antes de editar.
- Fazer a menor mudança coerente com a arquitetura existente.
- Adicionar ou atualizar testes unitários e de integração para cada comportamento alterado.
- Não descartar mudanças preexistentes nem alterar arquivos fora do escopo sem necessidade.
