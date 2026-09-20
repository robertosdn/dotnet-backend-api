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
