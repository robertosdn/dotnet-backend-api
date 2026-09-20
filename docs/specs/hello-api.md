# Especificacao: Hello API

## Status

Legada. Sera removida quando a primeira versao da gestao de usuarios de acesso estiver implementada e validada.

## Objetivo

Disponibilizar endpoints HTTP temporarios para validar o servidor, roteamento, resposta JSON e eco de payload durante o bootstrap do projeto.

## Requisitos

- `GET /hello` deve retornar `200` e `{"message": "Hello, World!"}`.
- `GET /hello/` deve retornar `200` e `{"message": "Hello, stranger!"}`.
- `GET /hello/{name}` deve retornar `200` e uma saudacao com o nome informado.
- Caminhos de nome vazios ou contendo barras adicionais devem retornar `404`.
- `POST /echo` deve retornar `200` com o corpo recebido.
- Rotas desconhecidas devem retornar `404` e `Not Found\\n`.
- Respostas JSON devem informar `Content-Type: application/json`.

## CQRS

Esta especificacao nao persiste estado. As saudacoes e o eco sao queries ou operacoes sem command de escrita; portanto, nao geram eventos de integracao nem registros na outbox.

## Retirada

Os endpoints desta especificacao nao fazem parte do produto final. A remocao deve ocorrer depois que `docs/specs/access-user-management.md` estiver implementada, com os testes HTTP de demonstracao removidos ou substituidos pelos testes dos endpoints reais.

## Testes Atuais

Os fluxos HTTP legados devem ser cobertos em `Backend.Api.IntegrationTests/Http/HelloEndpointTests.cs` por testes de integracao usando `WebApplicationFactory` sem iniciar um processo externo separado. Esta cobertura deve ser removida junto com os endpoints de demonstracao.
