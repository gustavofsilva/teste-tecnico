# ADR 0003 — Deploy gratuito com PostgreSQL

## Estado

Aceito para o ambiente de demonstração hospedado.

## Contexto

O desafio exige execução reproduzível com Docker e permite SQLite, SQL Server ou MySQL. O Compose existente usa SQLite e permanece a referência de avaliação. Para disponibilizar uma demonstração gratuita, o filesystem do Render Free não oferece persistência adequada para o arquivo SQLite.

## Decisão

- Manter SQLite como provider padrão local e no Docker Compose.
- Permitir PostgreSQL por configuração exclusivamente para o deploy de demonstração.
- Hospedar Angular na Vercel, API no Render e PostgreSQL no Supabase.
- Escolher o provider com `DatabaseProvider`, sem credenciais no código.
- Incorporar a URL pública da API no build Angular por `API_URL`.

## Consequências

- A entrega Docker continua usando uma tecnologia de banco explicitamente aceita pelo desafio.
- A demonstração mantém dados apesar dos reinícios do backend gratuito.
- O backend passa a carregar dois providers EF Core.
- Alterações de esquema precisam considerar ambos os providers.
- O Supabase e o Render podem pausar recursos gratuitos por inatividade.
