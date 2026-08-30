# Deploy gratuito — Vercel, Render e Supabase

## Arquitetura

```text
Navegador → Vercel/Angular → Render/ASP.NET Core → Supabase/PostgreSQL
```

SQLite permanece como provider padrão local e no Docker Compose. PostgreSQL é habilitado somente no Render com `DatabaseProvider=PostgreSql`.

## 1. Publicar o repositório

Vercel e Render fazem deploy a partir de um repositório Git remoto. Antes de continuar, confirme que o código está em um repositório GitHub e que nenhum `.env`, token ou connection string real foi commitado.

## 2. Criar o banco no Supabase

1. Crie um projeto gratuito no Supabase.
2. Defina e armazene uma senha forte para o banco.
3. No projeto, abra **Connect**.
4. Selecione a conexão **Session pooler**, porta `5432`, indicada para um backend persistente em rede IPv4.
5. Copie a URI no formato `postgresql://...`.

Não coloque essa URI em arquivos do repositório. Ela concede acesso ao banco.

## 3. Publicar a API no Render

1. No Render, escolha **New → Blueprint** e conecte o repositório.
2. O arquivo `render.yaml` criará o web service `minha-conta-api` pelo Dockerfile existente.
3. Preencha as variáveis marcadas como secretas:

| Variável | Valor |
|---|---|
| `ConnectionStrings__DefaultConnection` | URI Session pooler copiada do Supabase |
| `FrontendUrl` | URL final da Vercel, por exemplo `https://minha-conta.vercel.app` |

As demais variáveis são definidas pelo Blueprint. `Jwt__Key` é gerada pelo Render.

4. Execute o deploy e aguarde `/health` responder:

```text
https://SUA-API.onrender.com/health
```

Na primeira inicialização, a API aplica a migration EF Core e cria a tabela `Users` e seu índice único. O Supabase já possui tabelas internas, por isso o ambiente PostgreSQL usa `MigrateAsync`; `EnsureCreated` permanece restrito ao SQLite local.

## 4. Publicar o Angular na Vercel

1. Importe o mesmo repositório na Vercel.
2. Defina **Root Directory** como `frontend`.
3. Cadastre a variável de ambiente de build:

| Variável | Valor |
|---|---|
| `API_URL` | URL da API sem barra final, por exemplo `https://SUA-API.onrender.com` |

4. Faça o deploy. `vercel.json` define o build Angular e o diretório de saída.
5. Copie o domínio de produção da Vercel para `FrontendUrl` no Render e faça novo deploy da API caso o valor ainda não estivesse correto.

O script `frontend/scripts/write-environment.mjs` incorpora `API_URL` no bundle durante o build. Nenhum segredo deve ser colocado nessa variável: URLs públicas aparecem no JavaScript entregue ao navegador.

## 5. Validar

1. Abra `https://SUA-API.onrender.com/health` e confirme `{"status":"healthy"}`.
2. Abra a aplicação na Vercel.
3. Cadastre uma conta nova.
4. Confirme a saudação no dashboard.
5. Edite nome, email e senha.
6. Saia e entre com a nova credencial.
7. Aguarde um novo deploy/restart do Render e confirme que a conta continua disponível no Supabase.

## Limites gratuitos

- O Render Free pode suspender a API após inatividade; a primeira requisição pode demorar.
- O Supabase Free pode pausar projetos com pouca atividade; retome o projeto pelo painel antes de uma apresentação.
- O frontend da Vercel continua disponível mesmo enquanto API ou banco despertam.
