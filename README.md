# Minha Conta — desafio full-stack

Aplicação web para cadastro, autenticação e manutenção do próprio perfil, desenvolvida para o desafio Full-Stack AI First / SDD.

O projeto possui duas aplicações independentes:

- **Frontend:** Angular 18 standalone, TypeScript, formulários reativos e Angular Router.
- **Backend:** API ASP.NET Core 8, Entity Framework Core, SQLite por padrão e autenticação JWT.

O ASP.NET Core é utilizado somente no backend. O frontend é uma SPA Angular executada pelo servidor de desenvolvimento do Angular ou servida pelo Nginx no ambiente Docker.

## Funcionalidades

- Cadastro com validação de nome, email, senha e confirmação.
- Login com email e senha.
- Dashboard protegido com identificação do usuário autenticado.
- Consulta e edição do próprio nome, email e senha.
- Proteção de rotas no Angular e de endpoints na API.
- Normalização e unicidade de email.
- Hash seguro de senha e autenticação JWT.
- Feedback de carregamento, sucesso e erro.

## Tecnologias

### Frontend

- Angular 18
- TypeScript 5.5 em modo estrito
- Angular Router
- Reactive Forms
- Signals
- RxJS
- `zone.js`

### Backend

- .NET 8 / ASP.NET Core 8
- Minimal APIs
- Entity Framework Core 8
- SQLite para execução local e Docker do desafio
- PostgreSQL/Supabase opcional para a demonstração hospedada
- JWT Bearer
- `PasswordHasher<T>` do ASP.NET Core Identity
- xUnit e `WebApplicationFactory`

### Infraestrutura e documentação

- Docker Compose opcional para execução reproduzível
- Nginx como servidor do frontend e proxy reverso no contêiner
- Especificações SDD e registros de decisões arquiteturais

> **Banco de dados:** SQLite continua sendo o padrão local e do Compose, preservando a solução pedida pelo desafio. No deploy gratuito, o backend pode usar PostgreSQL no Supabase por configuração. Uma instalação local de MySQL não é utilizada.

## Executar localmente sem Docker

### Pré-requisitos

- .NET SDK 8
- Node.js com npm

Não é necessário instalar Angular CLI globalmente nem configurar um servidor de banco. O Angular CLI está nas dependências do projeto e o arquivo SQLite é criado automaticamente.

### 1. Iniciar o backend

Em um terminal PowerShell, a partir da raiz do repositório:

```powershell
cd .\backend\UserProfile.Api
$env:ASPNETCORE_URLS = "http://localhost:8080"
dotnet run --no-launch-profile
```

A API ficará disponível em `http://localhost:8080`.

### 2. Iniciar o frontend

Em outro terminal PowerShell:

```powershell
cd .\frontend
npm install
npm start -- --proxy-config proxy.conf.json
```

O proxy de desenvolvimento encaminha `/api` e `/health` para o backend na porta `8080`.

### 3. Acessar

- Aplicação: **http://localhost:4200**
- Login: **http://localhost:4200/login**
- Cadastro: **http://localhost:4200/cadastro**
- Health check pelo frontend: **http://localhost:4200/health**
- Health check direto da API: **http://localhost:8080/health**

Resposta esperada do health check:

```json
{"status":"healthy"}
```

### Usuário para login

O repositório não possui usuário ou senha padrão e não cria seed de contas. No primeiro acesso, utilize a tela **Cadastre-se**.

Uma conta criada localmente fica armazenada no arquivo SQLite e permanece disponível nas próximas execuções. Para a instância local usada durante o desenvolvimento foi criada esta conta de demonstração:

```text
Email: teste@teste.com
Senha: 123456
```

Essa credencial não existe automaticamente em uma instalação nova.

## Executar com Docker — opcional

O Docker não é necessário para o desenvolvimento local, mas continua disponível para atender à exigência de execução reproduzível do desafio:

```bash
docker compose up --build
```

Acesse `http://localhost:4200` e crie uma conta pela tela de cadastro.

Para encerrar:

```bash
docker compose down
```

Para encerrar e apagar também o volume com os usuários cadastrados:

```bash
docker compose down -v
```

As configurações opcionais estão documentadas em `.env.example`:

- `WEB_PORT`: porta pública do frontend; padrão `4200`.
- `JWT_KEY`: chave JWT com pelo menos 32 bytes.

A chave padrão do Compose serve apenas para demonstração local e não deve ser usada em produção.

## Publicação gratuita

A demonstração pode ser publicada com:

```text
Frontend Angular → Vercel
API ASP.NET Core → Render Free
Banco hospedado → Supabase PostgreSQL Free
```

O provider é selecionado por `DatabaseProvider`: `Sqlite` é o padrão; `PostgreSql` ativa Npgsql. Instruções completas estão em [deploy Vercel, Render e Supabase](docs/DEPLOY.md).

## Testes e validação

### Backend

Com o .NET SDK instalado:

```powershell
dotnet test .\backend\UserProfile.sln --configuration Release
```

A suíte possui 20 casos aprovados e cobre:

- Jornada de cadastro, perfil autenticado, edição, troca de senha e novo login.
- Validações de cadastro e edição, incluindo detalhes por campo.
- Duplicidade e concorrência real de cadastro do mesmo email.
- Credenciais inválidas e conteúdo das respostas de erro.
- Normalização de caixa e espaços no email.
- Manutenção da senha quando o campo de edição fica vazio.
- Autorização do perfil.
- JWT expirado, adulterado ou com emissor/audiência inválidos.
- Contrato do health check.

### Build do frontend

```powershell
cd .\frontend
npm run build -- --configuration production
```

O build utiliza verificação estrita de TypeScript e templates Angular. O frontend ainda não possui uma suíte automatizada de componentes ou testes E2E; essa é uma evolução recomendada.

### Smoke test do Compose

Quando Docker estiver disponível:

```powershell
.\scripts\smoke-compose.ps1
```

O script cria um ambiente temporário, realiza build, aguarda o health check, testa cadastro, perfil autenticado e login, e remove os contêineres e o volume ao terminar.

## Estrutura do repositório

```text
backend/
  UserProfile.Api/         API ASP.NET Core, domínio, EF Core, JWT e Dockerfile
  UserProfile.Api.Tests/   testes HTTP integrados com xUnit
frontend/                  aplicação Angular, proxy local, Nginx e Dockerfile
docs/sdd/                  requisitos, design, plano, estratégia e validação
docs/adr/                  registros de decisões arquiteturais
scripts/                   automações de validação
compose.yaml               ambiente Docker opcional e volume persistente
```

## Arquitetura e decisões principais

- **Separação frontend/backend:** Angular cuida exclusivamente da interface; ASP.NET Core expõe os contratos HTTP e regras confiáveis do servidor.
- **API vertical e enxuta:** contratos, domínio, dados, segurança e endpoints possuem responsabilidades distintas sem camadas artificiais.
- **Providers por ambiente:** SQLite mantém a execução local/Docker alinhada ao desafio; PostgreSQL permite persistência gratuita no Supabase para a demonstração hospedada.
- **Email único:** normalização na aplicação e índice único no banco evitam duplicidade, inclusive em requisições concorrentes.
- **JWT HS256:** valida assinatura, emissor, audiência e expiração. A identidade do perfil vem do claim `sub` assinado.
- **Hash de senha:** `PasswordHasher<T>` aplica algoritmo lento, salt e formato versionado; a senha nunca é persistida ou retornada em texto puro.
- **Angular standalone:** formulários reativos centralizam validações, o guard protege rotas e o interceptor inclui o token nas chamadas.
- **Proxy por ambiente:** o Angular Dev Server encaminha chamadas durante o desenvolvimento; no contêiner, essa função pertence ao Nginx.

Detalhes adicionais:

- [Requisitos](docs/sdd/01-requisitos.md)
- [Design técnico](docs/sdd/02-design-tecnico.md)
- [Plano e rastreabilidade](docs/sdd/03-plano.md)
- [Estratégia de testes](docs/sdd/04-estrategia-testes.md)
- [Relatório de validação](docs/sdd/05-relatorio-validacao.md)
- [Registros de arquitetura](docs/adr/)

## Segurança e limites conhecidos

- O token é armazenado em `localStorage` por simplicidade. Em produção, cookies `HttpOnly`, `Secure` e `SameSite`, acompanhados de proteção CSRF, reduziriam a exposição a XSS.
- A inicialização usa `EnsureCreated`. Um ambiente produtivo deve utilizar migrations controladas.
- Não há rate limiting, bloqueio progressivo de login, refresh token ou revogação de sessão.
- SQLite é adequado ao escopo local e ao Compose; a demonstração hospedada usa PostgreSQL porque o filesystem gratuito do Render é efêmero.
- A chave JWT de demonstração deve ser substituída em qualquer ambiente real.

## Processo AI First / SDD

O desenvolvimento foi orientado pela sequência:

```text
requisitos → critérios de aceite → design técnico → plano → implementação → testes → validação
```

Os artefatos em `docs/sdd` registram requisitos, contratos, rastreabilidade, estratégia de testes e resultados. Os ADRs preservam o contexto e os trade-offs das decisões arquiteturais. O uso de IA apoiou decomposição, implementação e revisão, mas a correção, segurança e compreensão da entrega continuam sendo responsabilidade do desenvolvedor.
