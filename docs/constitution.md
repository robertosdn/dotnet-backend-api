# Constituição do Projeto

## Objetivo

Manter uma API backend completa em C# sobre .NET 10 LTS, testável e preparada para evoluir com múltiplas features sem perder clareza arquitetural.

## Princípios Obrigatórios

1. **C#/.NET idiomático**: preferir tipos fortes, nullable reference types, erros explícitos, async/await e responsabilidades pequenas.
2. **CQRS**: separar operacoes que alteram estado (commands) de operacoes que apenas consultam estado (queries).
3. **Transactional Outbox**: quando uma operação alterar estado e gerar evento de integração, persistir ambos na mesma transação atômica antes de publicar o evento.
4. **Idempotência**: comandos e o processamento da outbox devem tolerar retries sem duplicar efeitos.
5. **Testes segmentados**: toda funcionalidade nova ou alterada deve ter testes unitários e, quando aplicável, testes de integração em classes separadas nos projetos `*.Tests`; não concentrar a suíte em uma classe monolítica.
6. **Docker**: compilar, testar e executar a aplicação preferencialmente por Docker Compose ou pelas etapas do Dockerfile.
7. **Mudanças pequenas**: evitar refatorações fora do escopo e preservar contratos existentes.
8. **Modularidade verificável**: responsabilidades devem estar em projetos, namespaces e arquivos próprios conforme o plano; `Program.cs` é ponto de composição, não local para regras de negócio ou infraestrutura.
9. **SOLID pragmático**: aplicar responsabilidade única, inversão de dependência e interfaces somente quando representarem portas, políticas ou variações reais.
10. **IoC explícito**: registrar dependências por extensões de `IServiceCollection`; evitar service locator, estado global e dependências concretas nos casos de uso.
11. **API .NET idiomática**: usar Minimal APIs, grupos versionados em `/api/v1`, `ProblemDetails`, OpenAPI e `CancellationToken` em operações de I/O.

## Definition Of Done

Uma mudanca so esta concluida quando:

- a especificação correspondente foi atualizada;
- commands, queries e eventos estão separados conforme aplicável;
- a estrutura de projetos prevista no plano existe e `Program.cs` contém somente composição do host e registro de dependências/rotas;
- os projetos dependem em uma única direção e `Backend.Domain` não depende de ASP.NET Core ou infraestrutura;
- os testes unitários e de integração relevantes foram adicionados ou atualizados;
- os testes estão segmentados por camada nos projetos `*.Tests`, sem classe monolítica de testes;
- `dotnet format --verify-no-changes`, `dotnet test` e `dotnet build --warnaserror` foram executados no Docker quando suportados pelo ambiente;
- riscos, limitacoes e comandos executados foram registrados no resumo da mudanca.
