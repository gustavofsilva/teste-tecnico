# Explicação e justificativas do projeto

## Visão geral

“Minha Conta” é uma aplicação full-stack que implementa os quatro fluxos pedidos pelo `desafio.md`: cadastro, login, dashboard e consulta/edição do próprio perfil.

A solução é dividida em duas aplicações:

- O **frontend é uma SPA Angular 18**, responsável pela interface, navegação, formulários e experiência de autenticação.
- O **backend é uma API ASP.NET Core 8**, responsável pelas regras confiáveis, persistência, hash de senha, emissão e validação de JWT.

Essa separação é importante: ASP.NET Core não renderiza o frontend. Em desenvolvimento, o Angular Dev Server serve a interface e encaminha `/api` ao backend por meio de `proxy.conf.json`. No ambiente Docker, o Nginx serve o build Angular e atua como proxy reverso.

## Aderência tecnológica ao desafio

| Exigência do `desafio.md` | Tecnologia implementada | Estado |
|---|---|---|
| Backend em .NET Core/C# | ASP.NET Core 8 com C# e Minimal APIs | Atendido |
| Entity Framework Core | EF Core 8 com provider SQLite | Atendido |
| SQLite, SQL Server ou MySQL | SQLite | Atendido |
| Autenticação JWT | JWT Bearer com assinatura HS256 | Atendido |
| Frontend Angular | Angular 18 standalone | Atendido |
| Framework de UI | CSS próprio; o framework é opcional no desafio | Atendido |
| Rotas de login, cadastro, dashboard e perfil | Angular Router com carregamento sob demanda | Atendido |
| Proteção das rotas privadas | Guard no Angular e autorização na API | Atendido |
| Docker Compose na raiz | `compose.yaml` com frontend, backend e volume SQLite | Atendido na implementação; smoke test real ainda deve ser executado em máquina com Docker |
| Processo AI First / SDD | Requisitos, design, plano, testes, validação e ADRs em `docs/` | Conteúdo presente, mas ainda não versionado: o repositório não possui commits |

O MySQL instalado na máquina não é utilizado. Isso não representa desvio, pois o desafio aceita explicitamente SQLite, SQL Server ou MySQL. SQLite reduz dependências e permite que o Compose suba a solução sem um terceiro serviço de banco.

Para a demonstração pública gratuita, existe um provider PostgreSQL opcional conectado ao Supabase. Essa extensão não substitui a entrega de referência: execução local, testes e Docker Compose continuam usando SQLite. O provider é escolhido por configuração, sem alterar os contratos da API.

## Por que estas tecnologias

### ASP.NET Core 8 no backend

ASP.NET Core 8 atende à exigência de Web API em .NET e oferece autenticação, autorização, injeção de dependência, configuração e pipeline HTTP integrados. Minimal APIs foram escolhidas porque o domínio possui poucos casos de uso. Os endpoints não ficam no `Program.cs`: contratos, domínio, persistência, segurança e endpoints estão separados por responsabilidade.

Embora o texto do desafio use o nome histórico “.NET Core”, o target `net8.0` e o SDK `Microsoft.NET.Sdk.Web` representam a plataforma moderna .NET/ASP.NET Core compatível com essa exigência.

### Entity Framework Core com SQLite

EF Core implementa a persistência solicitada. SQLite foi escolhido entre as opções expressamente permitidas porque:

- fornece persistência relacional real;
- suporta chave primária e índice único de email;
- não exige servidor ou credenciais de banco;
- simplifica execução local e em contêiner;
- mantém os dados Docker em volume nomeado.

A aplicação usa `EnsureCreated` para inicializar o SQLite no escopo local do desafio. O PostgreSQL hospedado usa migration versionada e `MigrateAsync`, pois o Supabase já contém tabelas internas e não é um banco vazio.

### JWT para autenticação

A API emite JWT assinado com HS256 e valida:

- assinatura;
- emissor;
- audiência;
- expiração;
- chave mínima de 32 bytes.

O identificador usado para consultar o perfil vem do claim `sub` assinado. O cliente não envia um ID de usuário para escolher qual perfil será lido ou alterado, reduzindo o risco de IDOR.

### Angular 18 no frontend

Angular atende diretamente à tecnologia obrigatória do frontend. A aplicação usa:

- componentes standalone;
- Angular Router;
- rotas carregadas sob demanda;
- formulários reativos;
- validações de formulário;
- signals para estado de sessão;
- interceptor para o header Bearer;
- guard para rotas privadas;
- `zone.js` para o mecanismo padrão de detecção de mudanças.

Não foi adotado Angular Material, PrimeNG ou outro framework visual porque o desafio os define como opcionais. O CSS próprio é suficiente para o tamanho da interface e evita dependências sem necessidade funcional.

### Nginx e proxy local

Nginx pertence apenas à imagem final do frontend. Ele serve os arquivos estáticos gerados pelo Angular, permite refresh de rotas da SPA e encaminha `/api` e `/health` ao serviço ASP.NET Core.

Na execução sem Docker, `ng serve` utiliza `proxy.conf.json` para encaminhar as mesmas rotas ao backend em `http://localhost:8080`. Assim, o código Angular utiliza URLs relativas e não precisa incorporar uma URL de API específica no bundle.

### Docker Compose

O `compose.yaml` atende à estrutura exigida pelo desafio:

- constrói a API ASP.NET Core;
- constrói e serve o frontend Angular com Nginx;
- publica a interface na porta `4200` por padrão;
- cria volume persistente para o SQLite;
- externaliza porta e chave JWT;
- oferece health check pela rota `/health`.

Os Dockerfiles são multi-stage: SDK .NET e toolchain Node ficam somente nas etapas de build. A imagem final da API usa o runtime ASP.NET e executa como usuário não-root; a imagem final do frontend usa Nginx.

O empacotamento foi inspecionado estaticamente e existe um smoke test automatizado em `scripts/smoke-compose.ps1`. A execução real do Compose continua uma validação pendente enquanto Docker não estiver disponível no ambiente de desenvolvimento. Essa pendência deve permanecer explícita, em vez de ser apresentada como evidência já produzida.

## Funcionalidades e critérios de aceite

### Cadastro

O cadastro recebe nome, email, senha e confirmação. Frontend e backend validam obrigatoriedade, formato e tamanhos mínimos. A API normaliza o email, rejeita duplicidade e transforma uma disputa concorrente pelo índice único em resposta `409`.

Em sucesso, a API retorna `201`, JWT e dados públicos do usuário. A interface cria a sessão e abre o dashboard. Em erro, exibe a mensagem retornada pela API ou uma mensagem genérica de contingência. O redirecionamento confirma o sucesso de forma implícita, mas o desafio pede uma mensagem explícita de sucesso; portanto, esse detalhe funcional ainda deve ser ajustado antes da entrega.

### Login

O login aceita email e senha, normaliza caixa e espaços externos do email e retorna mensagem genérica quando a credencial é inválida. Essa mensagem não informa se foi o email ou a senha que falhou, reduzindo enumeração de contas. Após sucesso, o Angular persiste a sessão e redireciona ao dashboard.

### Dashboard

O dashboard é protegido pelo guard e exibe uma saudação com o nome do usuário, seus dados públicos e acesso à edição do perfil. A API continua sendo a fronteira real de autorização; o guard melhora a experiência, mas não substitui `RequireAuthorization` no servidor.

### Edição de perfil

O usuário pode alterar nome, email e senha. Nome e email repetem as regras do cadastro. A senha é opcional na edição: vazia mantém o hash atual; preenchida exige no mínimo seis caracteres e confirmação igual. A interface mostra mensagens de sucesso ou erro após a operação.

## Segurança e consistência

A senha passa por `PasswordHasher<User>`, que aplica algoritmo lento, salt e formato versionado. Senhas não são persistidas nem devolvidas em texto puro. Quando o framework indica `SuccessRehashNeeded`, o login atualiza o hash de forma transparente.

O email é normalizado antes das consultas e possui índice único no banco. A consulta prévia melhora a resposta, enquanto a restrição do banco é a garantia final em concorrência. Contenções transitórias do SQLite recebem retry limitado; violações de unicidade são convertidas no contrato `409`.

As validações são duplicadas intencionalmente:

- no Angular, para feedback imediato;
- na API, porque o navegador não é uma fronteira de confiança;
- no banco, para invariantes estruturais como unicidade.

O JWT e o usuário atual ficam no `localStorage` por simplicidade e aderência ao exercício. O acesso inicial ao armazenamento é defensivo para não impedir o bootstrap em navegadores que o bloqueiem. Para produção, cookies `HttpOnly`, `Secure` e `SameSite`, acompanhados de proteção CSRF, reduziriam exposição em caso de XSS.

A chave JWT pode vir de variável de ambiente, o arquivo `.env` real é ignorado e `.env.example` contém somente exemplo. A chave padrão local não deve ser reutilizada em ambiente real.

## Qualidade e arquitetura

A arquitetura evita cerimônia desproporcional para um domínio de uma entidade. Não há repositório genérico sobre EF Core nem microsserviços sem necessidade. As fronteiras existentes têm objetivo concreto:

- `Contracts`: modelos de entrada e saída HTTP;
- `Domain`: entidade de usuário;
- `Data`: contexto e mapeamento do EF Core;
- `Security`: configuração e emissão de JWT;
- `Endpoints`: orquestração dos casos de uso;
- `frontend/core`: API, autenticação, guard, interceptor e modelos;
- `frontend/pages`: componentes associados às rotas.

O backend utiliza nullable reference types. O frontend usa TypeScript e templates estritos. Essas verificações de compilação reduzem erros, mas não substituem testes automatizados.

## Estratégia e evidências de teste

Os testes de integração usam `WebApplicationFactory`, pipeline HTTP real e SQLite compartilhado em memória, mantido durante a fixture. Eles verificam serialização, autenticação, autorização, persistência e regras sem substituir o provider relacional por mocks.

A suíte possui 20 casos aprovados, incluindo:

- jornada cadastro → perfil → edição/troca de senha → novo login;
- validações de cadastro e edição;
- preservação da senha quando vazia;
- normalização do email no login;
- conteúdo de mensagens e erros de validação;
- acesso sem token;
- JWT expirado, adulterado ou com issuer/audience inválidos;
- conflito de email na edição;
- duas requisições concorrentes para o mesmo cadastro;
- contrato do health check.

O frontend passa pelo build estrito do Angular, mas ainda não possui testes automatizados de componentes nem uma suíte E2E de navegador. Playwright seria a próxima evolução para cobrir redirecionamentos, mensagens e formulários pelo ponto de vista do usuário.

## Processo AI First / SDD

Os artefatos seguem a sequência exigida pelo desafio:

```text
requisitos e critérios de aceite
        ↓
design técnico e contratos
        ↓
plano e rastreabilidade
        ↓
implementação
        ↓
testes e relatório de validação
```

Os documentos em `docs/sdd` registram requisitos, design, plano, estratégia e resultados. Os ADRs preservam o contexto, alternativas e consequências das decisões sobre arquitetura, persistência e autenticação.

AI First não transfere responsabilidade à ferramenta. A implementação, os testes e as decisões precisam ser compreendidos e defendidos pelo candidato. O histórico completo de conversa não é necessário, mas os artefatos resultantes do processo devem permanecer coerentes e versionados junto ao código.

Neste momento, os artefatos existem no diretório de trabalho, porém o repositório Git ainda não possui commits. Assim, o conteúdo SDD está preparado, mas o requisito de versionamento e publicação no GitHub ainda não pode ser considerado concluído.

## Limitações e próximos passos

Para uma evolução de produção, os próximos passos prioritários seriam:

1. migrations controladas em vez de `EnsureCreated`;
2. rate limiting e proteção contra tentativas repetidas de login;
3. cookies HttpOnly e estratégia CSRF ou uma política formal para armazenamento de tokens;
4. refresh token com rotação e revogação;
5. testes Angular e testes E2E com Playwright;
6. logs estruturados, métricas e tracing;
7. banco servidor quando houver múltiplas réplicas ou maior concorrência;
8. pipeline CI para build, testes e validação do Compose.

## Pontos para explicar em uma entrevista

1. O frontend é Angular; ASP.NET Core é exclusivamente a API backend.
2. SQLite foi escolhido entre as opções permitidas para reduzir dependências operacionais.
3. O índice único, e não apenas a consulta anterior, garante unicidade do email.
4. O backend repete validações porque requisições podem ignorar o Angular.
5. O claim `sub` determina o perfil acessado e reduz risco de IDOR.
6. `PasswordHasher<T>` é preferível a algoritmos de senha implementados manualmente.
7. O guard protege a navegação, mas a autorização efetiva pertence à API.
8. JWT facilita autenticação stateless, mas revogação exige desenho adicional.
9. O Compose é requisito de entrega mesmo quando o desenvolvimento local ocorre sem Docker.
10. Artefatos SDD precisam refletir a implementação e as evidências reais, inclusive validações ainda pendentes.
