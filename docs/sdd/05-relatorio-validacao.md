# Relatório de validação

Data: 2026-08-29

## Resultado

| Área | Evidência | Estado |
|---|---|---|
| Cadastro, login e perfil | 20 testes HTTP integrados aprovados | Aprovado |
| Validações e autorização | Casos inválidos, ausência de token e conflitos cobertos | Aprovado |
| Frontend | Build Angular de produção com TypeScript e templates estritos | Aprovado |
| Segredos | `.env` ignorado; somente valores de demonstração e `.env.example` versionáveis | Aprovado |
| Docker Compose | Inspeção estática concluída; execução não realizada por indisponibilidade do Docker no ambiente de revisão | Pendente de smoke test |

## Defeitos encontrados e resolvidos

1. A suíte não compilava por ausência do namespace do xUnit.
2. Os DTOs posicionais não acionavam as validações esperadas; email inválido e senha curta chegavam ao endpoint. Os contratos agora usam propriedades explicitamente anotadas.
3. O estado de autenticação do Angular não era reativo após login/logout. O token agora possui estado por signal.
4. Respostas `401` com sessão existente agora limpam dados locais e redirecionam ao login.
5. Login agora atualiza hashes legados quando o framework indicar `SuccessRehashNeeded`.
6. Conflitos de unicidade concorrentes são convertidos em `409`, preservando o contrato HTTP.
7. Senha vazia na edição era rejeitada pela validação declarativa antes de alcançar a regra de preservação; a regra opcional agora é validada corretamente no endpoint.
8. A suíte passou a validar tokens expirados, adulterados e com issuer/audience inválidos, conteúdo dos erros, normalização de email, edição inválida, concorrência e health check.
9. Foi adicionado `scripts/smoke-compose.ps1` para validar automaticamente a aplicação completa e limpar seus recursos temporários.

## Comandos executados

```text
dotnet test UserProfile.sln --configuration Release --no-restore
Resultado: 20 aprovados, 0 falhas.

npm run build -- --configuration production
Resultado: build concluído; bundle inicial 225,99 kB.
```

## Validação restante

Em uma máquina com Docker, executar `docker compose up --build`, acessar `http://localhost:4200`, percorrer cadastro, edição e novo login e consultar `http://localhost:4200/health`. Essa é a única evidência não produzida nesta revisão.
