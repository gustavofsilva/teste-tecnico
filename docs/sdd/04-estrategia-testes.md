# Estratégia de testes e qualidade

## Pirâmide adotada

- **Integração da API:** `WebApplicationFactory` exercita HTTP, serialização, autenticação, regras e SQLite compartilhado em memória, isolado por fixture.
- **Build estático:** compilação .NET com nullable e Angular com TypeScript/templates estritos detecta incompatibilidades.
- **Smoke test em contêiner:** subir o Compose, consultar `/health`, cadastrar, autenticar, consultar e editar via interface/API.

## Cenários automatizados

1. Cadastro válido → token → consulta autenticada → edição com troca de senha → novo login.
2. Cadastro duplicado retorna conflito.
3. Senha incorreta retorna não autorizado.
4. Nome vazio, email inválido, senha curta e confirmação divergente retornam validação.
5. Perfil sem token retorna não autorizado.
6. Edição com email de outro usuário retorna conflito.
7. JWT expirado, adulterado ou com emissor/audiência inválidos retorna não autorizado.
8. Edição rejeita nome, email, senha e confirmação inválidos com detalhes por campo.
9. Senha vazia na edição mantém a credencial anterior.
10. Login normaliza caixa e espaços externos do email.
11. Respostas de conflito, credenciais e validação preservam o contrato documentado.
12. Dois cadastros simultâneos do mesmo email produzem exatamente um `201` e um `409`.
13. Health check retorna `200` e `{ "status": "healthy" }`.
14. Smoke test do Compose cobre build, inicialização, health, cadastro, perfil autenticado e login.

## Evidências esperadas

- `dotnet test`: 20 casos aprovados, incluindo segurança do JWT, concorrência e contratos de erro.
- `npm run build -- --configuration production`: compilação estrita de TypeScript e templates concluída.
- `docker compose config` e smoke test de `/health`: validação do empacotamento, quando Docker estiver disponível.

## Comandos

```bash
docker build -f backend/UserProfile.Api/Dockerfile backend
docker run --rm -v "$PWD/backend:/src" -w /src mcr.microsoft.com/dotnet/sdk:8.0 dotnet test UserProfile.sln
docker build frontend
docker compose config
docker compose up --build
powershell -File scripts/smoke-compose.ps1
```

Testes de navegador são a evolução recomendada (Playwright) se o produto ganhar fluxos e pipeline próprios. Para este escopo, os testes integrados concentram cobertura no maior risco: autorização, persistência e credenciais.
