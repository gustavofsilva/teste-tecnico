# Especificação funcional

## Escopo

Uma aplicação web permite criar uma conta, autenticar-se e consultar/alterar apenas o próprio perfil. Visitantes acessam login e cadastro; as demais rotas exigem JWT válido.

## Casos de uso e critérios de aceite

### UC-01 — Cadastrar usuário

- Nome, email, senha e confirmação são obrigatórios.
- Nome tem no mínimo 3 caracteres; email deve ser válido; senha tem no mínimo 6 caracteres.
- Senha e confirmação devem coincidir.
- Email é comparado sem distinção de maiúsculas após remoção de espaços externos e deve ser único.
- Em sucesso, a API responde `201`, autentica o usuário e a interface abre o dashboard.
- Duplicidade responde `409`; dados inválidos respondem `400`.

### UC-02 — Autenticar

- Credenciais válidas retornam JWT e dados públicos do usuário.
- Credenciais inválidas retornam `401` com mensagem genérica, sem revelar se o email existe.
- Em sucesso, a interface abre o dashboard.

### UC-03 — Visualizar dashboard

- Uma rota protegida exibe saudação com o nome e acesso à edição.
- Visitante é redirecionado ao login.

### UC-04 — Consultar e editar perfil

- O usuário autenticado consulta somente seu perfil.
- Nome e email seguem as regras do cadastro.
- Senha vazia mantém o hash atual; senha informada exige ao menos 6 caracteres e confirmação igual.
- Email pertencente a outra conta responde `409`.
- Sucesso exibe confirmação e atualiza o nome visível na sessão.

## Requisitos não funcionais

- Inicialização completa por `docker compose up`; persistência em volume Docker.
- Segredos configuráveis por ambiente e não versionados.
- Layout responsivo, feedback de carregamento/erro e campos com autocomplete adequado.
- Testes reproduzíveis dos principais fluxos da API.
