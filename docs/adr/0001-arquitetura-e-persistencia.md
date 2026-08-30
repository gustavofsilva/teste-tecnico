# ADR 0001 — Monólito modular e SQLite

- Status: aceito
- Data: 2026-08-21

## Decisão

Usar uma API ASP.NET Core modular por funcionalidade e SQLite via EF Core, com frontend Angular implantado separadamente no mesmo Compose.

## Motivos

O domínio tem uma entidade e quatro operações. Um monólito evita custo operacional e transações distribuídas sem impedir separação de responsabilidades. SQLite dispensa servidor adicional, oferece persistência relacional e torna a avaliação reproduzível. EF Core mantém mapeamento explícito e permite migrar para PostgreSQL/SQL Server se concorrência e escala crescerem.

## Consequências

A solução é simples de executar e manter. SQLite não é indicado para alta concorrência de escrita ou múltiplas réplicas da API; nessa situação, o provider e a conexão devem migrar para um SGBD servidor.
