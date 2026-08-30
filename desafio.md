Desafio de Contratação de Desenvolvedor Full-Stack (.NET Core +
Angular) - AI First / SDD
Objetivo: Desenvolver uma aplicação web completa utilizando .NET Core para o backend e
Angular para o frontend. A aplicação deve permitir o cadastro de usuários, autenticação via
login e fornecer uma interface para que o usuário possa visualizar e editar suas informações
cadastrais. O desafio deve ser desenvolvido seguindo o modelo AI First adotado pela
empresa, por meio de um processo baseado em Spec-Driven Development (SDD).
Requisitos:
Funcionalidades do Sistema

1. Tela de Login:
   ○ Usuário deve autenticar com email e senha.
   ○ Exibir mensagem de erro em caso de credenciais inválidas.
   ○ Redirecionar usuário autenticado para o dashboard.
2. Tela de Cadastro de Usuários:
   ○ Campos obrigatórios: Nome, Email, Senha, Confirmação de Senha.
   ○ Validações:
   ■ Nome: obrigatório e com no mínimo 3 caracteres.
   ■ Email: obrigatório e em formato válido.
   ■ Senha: obrigatória, com no mínimo 6 caracteres e confirmação deve
   coincidir.
   ○ Exibir mensagem de sucesso ou erro após tentativa de cadastro.
3. Dashboard:
   ○ Exibir uma frase de boas-vindas contendo o nome do usuário autenticado.
   ○ Disponibilizar menu de navegação para edição dos dados cadastrais.
4. Tela de Edição de Dados Cadastrais:
   ○ Permitir que o usuário edite suas informações: Nome, Email e Senha.
   ○ Validações semelhantes às do cadastro.
   ○ Exibir mensagem de sucesso ou erro após tentativa de edição.
   Tecnologias
   ● Backend:
   ○ .NET Core (C#)
   ○ Entity Framework Core
   ○ JWT para autenticação
   ● Frontend:
   ○ Angular
   ○ Framework de UI (opcional) – Ex.: PrimeNG, Angular Material, Ant Design,
   etc.
   Forma de Desenvolvimento - AI First / SDD
   ● O uso de ferramentas de Inteligência Artificial é permitido e esperado. A escolha da
   ferramenta, modelo ou agente é livre.
   ● O candidato continua responsável pela correção, segurança, qualidade e
   entendimento de tudo o que for entregue, independentemente do uso de IA.
   ● O desenvolvimento deve utilizar um processo baseado em Spec-Driven
   Development (SDD). Não é obrigatório adotar uma ferramenta ou framework de SDD
   específico.
   ● Todos os artefatos produzidos pelo processo SDD adotado devem ser versionados e
   entregues no mesmo repositório do código.
   ● Como referência, espera-se que o processo contemple, quando aplicável:
   ○ Especificação dos requisitos, casos de uso e/ou critérios de aceite.
   ○ Design técnico e decisões de arquitetura, incluindo contratos de API e
   modelo de dados quando relevantes.
   ○ Plano de implementação e decomposição em tarefas.
   ○ Estratégia de testes e demais artefatos de validação/qualidade gerados pelo
   processo.
   ○ Registros de decisões técnicas (ADRs ou equivalente), quando fizerem parte
   do processo adotado.
   ● Os artefatos podem ficar em uma estrutura como /docs/sdd, /specs ou na estrutura
   padrão da ferramenta utilizada. O importante é preservar os artefatos do processo e
   mantê-los coerentes com a implementação final.
   ● Não é necessário exportar o histórico completo de conversas com a IA. Não devem
   ser versionados segredos, credenciais ou informações sensíveis.
   Requisitos Não Funcionais
   ● Documentação do projeto (README) explicando como executar e validar a
   aplicação.
   ● Código bem estruturado, legível e com responsabilidades bem definidas.
   ● Boas práticas de programação e uso de controle de versão (Git).
   ● Disponibilizar a solução em um repositório público no GitHub.
   ● Implementar testes automatizados para os principais fluxos e regras da aplicação.
   ● Configurações e segredos devem ser externalizados; credenciais reais não devem
   ser versionadas.
   Execução com Docker
   ● O repositório deve conter um arquivo compose.yaml ou docker-compose.yml na raiz
   do projeto.
   ● A aplicação completa deve ser inicializada a partir da raiz do repositório com o
   comando docker compose up.
   ● O Docker Compose deve subir todos os componentes necessários para o
   funcionamento da solução, incluindo frontend, backend e banco de
   dados/persistência quando aplicável.
   ● Para executar o desafio, o avaliador deve precisar apenas de Docker e Docker
   Compose; não deve ser necessário instalar localmente .NET, Node.js, Angular CLI
   ou um banco de dados.
   ● Variáveis de ambiente necessárias devem estar documentadas e, quando aplicável,
   acompanhadas de um arquivo .env.example sem segredos reais.
   ● O README deve informar as URLs/portas da aplicação e qualquer dado ou
   credencial de teste necessário para validação.
   Diretrizes de Implementação
5. Backend (.NET Core)
   ○ Criar uma Web API em .NET Core.
   ○ Utilizar Entity Framework Core para persistência.
   ○ Utilizar SQLite, SQL Server ou MySQL. Caso seja utilizado um servidor de
   banco de dados, ele deve fazer parte do Docker Compose.
   ○ Implementar autenticação baseada em JWT.
   ○ A arquitetura e organização interna são livres, desde que estejam justificadas
   nos artefatos do SDD e sejam coerentes com o código entregue.
6. Frontend Angular
   ○ Criar a aplicação Angular com rotas para login, cadastro, dashboard e edição
   de perfil.
   ○ Implementar o fluxo de autenticação e proteção das rotas que exigem
   usuário autenticado.
   ○ Tratar estados de sucesso, erro e carregamento de forma adequada.
7. Processo SDD e Qualidade
   ○ Conduzir o trabalho a partir das especificações e critérios de aceite,
   evoluindo para design/plano, implementação e validação.
   ○ Manter os artefatos SDD atualizados quando decisões ou requisitos forem
   alterados durante o desenvolvimento.
   ○ Garantir consistência entre especificações, implementação e testes.
   ○ O candidato deve estar preparado para explicar as decisões tomadas,
   inclusive trechos produzidos ou auxiliados por IA.
   Entrega do Projeto
   ● Crie um repositório público no GitHub e disponibilize o código-fonte completo.
   ● Inclua no repositório todos os artefatos do processo SDD utilizado durante o
   desenvolvimento.
   ● Inclua o Docker Compose necessário para executar a solução completa com docker
   compose up.
   ● Inclua um README com instruções objetivas para subir, acessar, testar e encerrar o
   ambiente.
   ● Inclua exemplos ou seed de dados de teste para facilitar a validação do
   funcionamento, quando necessário.
   ● Inclua um .env.example se houver variáveis de ambiente configuráveis.
   Avaliação
   A avaliação será baseada nos seguintes critérios:
8. Funcionalidade: Todas as funcionalidades requisitadas foram implementadas e
   funcionam corretamente?
9. Processo SDD / AI First: Os artefatos estão completos, versionados, coerentes
   entre si e refletem o processo utilizado até a implementação?
10. Decisões Técnicas e Arquitetura: As decisões são adequadas ao problema e
    estão justificadas de forma clara?
11. Qualidade do Código: O código está bem estruturado, legível, seguro e segue boas
    práticas de desenvolvimento?
12. Testes e Qualidade: Os principais fluxos e regras estão cobertos por testes
    automatizados e são reproduzíveis?
13. Docker e Reprodutibilidade: O projeto sobe corretamente com docker compose up
    em um ambiente limpo, sem dependências locais adicionais?
14. Interface do Usuário: A interface é amigável, consistente e intuitiva?
15. Documentação: O README e os demais documentos permitem entender, executar
    e validar a solução com clareza?
16. Domínio da Solução: O candidato demonstra entendimento do que foi construído e
    consegue explicar/defender as decisões, inclusive as partes produzidas com apoio
    de IA?
    Boa sorte e bom desenvolvimento!
