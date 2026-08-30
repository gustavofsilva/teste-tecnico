# Design técnico

## Contexto e arquitetura

O frontend Angular é servido por Nginx, que encaminha `/api` ao serviço ASP.NET Core. A API autentica JWT e persiste usuários via EF Core em SQLite. Um volume mantém o banco entre reinicializações.

No ambiente opcional de demonstração pública, o Angular é servido pela Vercel, a API pelo Render e o EF Core usa PostgreSQL no Supabase. SQLite continua sendo o padrão local e do Compose; `DatabaseProvider` seleciona o provider sem alterar os casos de uso ou contratos HTTP.

```text
Navegador -> Nginx/Angular :4200 -> ASP.NET Core :8080 -> EF Core -> SQLite/volume
```

A API usa uma organização vertical pequena (`Endpoints`, `Contracts`, `Domain`, `Data`, `Security`). Ela evita camadas artificiais num domínio de uma entidade, mas separa contratos HTTP, persistência e segurança.

## Modelo de dados

`User(Id UUID PK, Name varchar(120), Email varchar(254) UNIQUE, PasswordHash text, CreatedAt, UpdatedAt)`.

Senhas nunca são retornadas ou persistidas em texto. `PasswordHasher<T>` aplica PBKDF2 com salt e formato versionado. O índice único é a garantia final contra corrida de duplicidade.

## Contrato HTTP

| Método | Rota | Autenticação | Resultado |
|---|---|---|---|
| POST | `/api/auth/register` | Não | `201 AuthResponse`; `400`; `409` |
| POST | `/api/auth/login` | Não | `200 AuthResponse`; `401` |
| GET | `/api/profile` | Bearer | `200 UserResponse`; `401` |
| PUT | `/api/profile` | Bearer | `200 UserResponse`; `400`; `401`; `409` |
| GET | `/health` | Não | `200` |

`AuthResponse = { token, user: { id, name, email } }`. Erros de negócio têm `{ message }`; erros de validação usam `ValidationProblemDetails`.

## Segurança

- JWT HS256 valida assinatura, emissor, audiência e expiração; chave mínima de 32 bytes.
- Identidade do perfil vem exclusivamente do `sub` do token, nunca do corpo ou URL.
- Mensagem de login deliberadamente genérica reduz enumeração de contas.
- CORS possui origem explícita; Nginx fornece mesma origem no fluxo Docker.
- O token em `localStorage` atende ao escopo do desafio. Em produção com backend controlado, recomenda-se cookie `HttpOnly`, `Secure`, `SameSite` e proteção CSRF para reduzir exposição a XSS.
