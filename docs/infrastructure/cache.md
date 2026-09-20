# Infraestrutura: Redis

## Status

Aprovada somente para sessões e dados temporários, fora do read model de CQRS.

## Responsabilidade

Redis será usado para sessões, tokens revogados, rate limiting e outros dados temporários. Redis não será usado para consultas do read model de usuários e nunca será fonte de verdade dos usuários ou das transações.

## Regras

- Toda chave deve possuir namespace e versão, por exemplo `session:v1:{id}`.
- Toda entrada deve possuir TTL explícito, salvo decisão documentada.
- O sistema deve continuar correto quando o Redis estiver indisponível.
- A indisponibilidade do Redis deve bloquear apenas o recurso temporário que depende dele; queries de usuários não devem tentar usá-lo.
- Invalidação ou atualização de sessões deve ocorrer conforme o ciclo de vida da sessão.
- Não armazenar senha em texto puro.
- Segredos, tokens e sessões devem ter política de expiração e revogação.

## Consistencia

O MySQL permanece como write model e o Elasticsearch como read model. Redis não faz parte do caminho de consulta dos usuários.

## Testes

- Criação, renovação, revogação e expiração de sessões.
- Redis indisponível sem corromper os dados persistidos.
- Rate limiting e tokens revogados, quando implementados.
- Rate limiting e revogação de credencial, quando implementados.
