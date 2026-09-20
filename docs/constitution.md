# Constituicao do Projeto

## Objetivo

Manter uma API backend completa em C# sobre .NET 10 LTS, testavel e preparada para evoluir com multiplas features sem perder clareza arquitetural.

## Principios Obrigatorios

1. **C#/.NET idiomatico**: preferir tipos fortes, nullable reference types, erros explicitos, async/await e responsabilidades pequenas.
2. **CQRS**: separar operacoes que alteram estado (commands) de operacoes que apenas consultam estado (queries).
3. **Transactional Outbox**: quando uma operacao alterar estado e gerar evento de integracao, persistir ambos na mesma transacao atomica antes de publicar o evento.
4. **Idempotencia**: comandos e o processamento da outbox devem tolerar retries sem duplicar efeitos.
5. **Testes segmentados**: toda funcionalidade nova ou alterada deve ter testes unitarios e, quando aplicavel, testes de integracao em classes separadas nos projetos `*.Tests`; nao concentrar a suite em uma classe monolitica.
6. **Docker**: compilar, testar e executar a aplicacao preferencialmente por Docker Compose ou pelas etapas do Dockerfile.
7. **Mudancas pequenas**: evitar refatoracoes fora do escopo e preservar contratos existentes.
8. **Modularidade verificavel**: responsabilidades devem estar em projetos, namespaces e arquivos proprios conforme o plano; `Program.cs` e ponto de composicao, nao local para regras de negocio ou infraestrutura.
9. **SOLID pragmatico**: aplicar responsabilidade unica, inversao de dependencia e interfaces somente quando representarem portas, politicas ou variacoes reais.
10. **IoC explicito**: registrar dependencias por extensoes de `IServiceCollection`; evitar service locator, estado global e dependencias concretas nos casos de uso.
11. **API .NET idiomatica**: usar Minimal APIs, grupos versionados em `/api/v1`, `ProblemDetails`, OpenAPI e `CancellationToken` em operacoes de I/O.

## Definition Of Done

Uma mudanca so esta concluida quando:

- a especificacao correspondente foi atualizada;
- commands, queries e eventos estao separados conforme aplicavel;
- a estrutura de projetos prevista no plano existe e `Program.cs` contem somente composicao do host e registro de dependencias/rotas;
- os projetos dependem em uma unica direcao e `Backend.Domain` nao depende de ASP.NET Core ou infraestrutura;
- os testes unitarios e de integracao relevantes foram adicionados ou atualizados;
- os testes estao segmentados por camada nos projetos `*.Tests`, sem classe monolitica de testes;
- `dotnet format --verify-no-changes`, `dotnet test` e `dotnet build --warnaserror` foram executados no Docker quando suportados pelo ambiente;
- riscos, limitacoes e comandos executados foram registrados no resumo da mudanca.
