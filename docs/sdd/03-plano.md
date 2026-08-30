# Plano de implementação

1. **Concluído** — Traduzir requisitos em casos de uso e critérios de aceite.
2. **Concluído** — Definir arquitetura, contrato HTTP, dados e decisões de segurança.
3. **Concluído** — Implementar entidade, persistência, autenticação e endpoints.
4. **Concluído** — Implementar rotas Angular, formulários, sessão, guard e interceptor.
5. **Concluído** — Criar imagens Docker, proxy e composição com persistência.
6. **Concluído** — Automatizar testes dos fluxos críticos e documentar validação.

## Rastreabilidade

| Critério | Implementação | Validação |
|---|---|---|
| UC-01 | `Register`, `RegisterComponent` | cadastro no teste de jornada; duplicidade |
| UC-02 | `Login`, `LoginComponent` | login após troca de senha; credencial inválida |
| UC-03 | `authGuard`, `DashboardComponent` | inspeção/build do frontend |
| UC-04 | endpoints de perfil, `ProfileComponent` | GET/PUT no teste de jornada |

## Registro da revisão AI-first

A revisão de 2026-08-29 confrontou especificação, código e testes. Ela identificou e corrigiu o estado de autenticação não reativo no Angular, acrescentou expiração de sessão ao receber `401`, rehash transparente de senha e tratamento defensivo da corrida de unicidade de email. A suíte passou a verificar também todas as classes de validação do cadastro, acesso anônimo ao perfil e conflito de email na edição.

A revisão de 2026-08-30 ampliou a validação para JWT inválido/expirado, regras de edição, preservação de senha, normalização de email, contratos de erro, cadastro concorrente e health check. Um smoke test automatizado do Compose passou a cobrir a jornada pela aplicação empacotada.

A adaptação de deploy manteve SQLite como referência local/Compose e adicionou PostgreSQL configurável para Supabase, Render Blueprint, URL pública de API no build Vercel e documentação operacional em `docs/DEPLOY.md`.
