# Infraestrutura: Redis

## Status

Aprovada somente para sessoes e dados temporarios, fora do read model de CQRS.

## Responsabilidade

Redis sera usado para sessoes, tokens revogados, rate limiting e outros dados temporarios. Redis nao sera usado para consultas do read model de usuarios e nunca sera fonte de verdade dos usuarios ou das transacoes.

## Regras

- Toda chave deve possuir namespace e versao, por exemplo `session:v1:{id}`.
- Toda entrada deve possuir TTL explicito, salvo decisao documentada.
- O sistema deve continuar correto quando o Redis estiver indisponivel.
- A indisponibilidade do Redis deve bloquear apenas o recurso temporario que depende dele; queries de usuarios nao devem tentar usa-lo.
- Invalidacao ou atualizacao de sessoes deve ocorrer conforme o ciclo de vida da sessao.
- Nao armazenar senha em texto puro.
- Segredos, tokens e sessoes devem ter politica de expiracao e revogacao.

## Consistencia

O MySQL permanece como write model e o Elasticsearch como read model. Redis nao faz parte do caminho de consulta dos usuarios.

## Testes

- Criacao, renovacao, revogacao e expiracao de sessoes.
- Redis indisponivel sem corromper os dados persistidos.
- Rate limiting e tokens revogados, quando implementados.
- Rate limiting e revogacao de credencial, quando implementados.
