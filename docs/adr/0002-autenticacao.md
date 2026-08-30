# ADR 0002 — JWT e hash de senha

- Status: aceito
- Data: 2026-08-21

## Decisão

Usar JWT HS256 curto (60 minutos), `PasswordHasher<T>` para hash e identidade extraída do claim `sub`.

## Motivos e consequências

JWT atende diretamente ao requisito e mantém a API stateless. O hasher oficial evita algoritmo criptográfico artesanal e suporta evolução do formato. Tokens emitidos não podem ser revogados individualmente nesta versão; logout remove a sessão do cliente. Refresh tokens, rotação/revogação e cookies HttpOnly são evoluções para um produto real.
