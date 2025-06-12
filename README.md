# 🔐 SecureTaskManager

Sistema de Autenticação e Gerenciamento de Usuários com ASP.NET Core + Blazor WebAssembly + JWT.

## 🧠 Sobre o Projeto

Este projeto é uma aplicação full-stack que oferece:

- Cadastro e login de usuários
- Autenticação via JWT
- Controle de acesso baseado em roles (`Admin` e `User`)
- Permissão para o usuário editar perfil e alterar senha
- Endpoint para autoexclusão de conta
- Interface moderna com Blazor WebAssembly

Projeto criado com fins educacionais e portfólio.

## 💡 Tecnologias Utilizadas

### Backend (.NET 8 - ASP.NET Core API)
- ASP.NET Core Web API
- Identity com Entity Framework Core
- Autenticação JWT
- SQLite como banco de dados
- Repositório e camada de serviço

### Frontend (Blazor WebAssembly)
- Blazor WebAssembly (SPA)
- Consumo de API com `HttpClient`
- Controle de autenticação com `JwtAuthenticationStateProvider`
- Interface dinâmica com componentes Blazor

## 📁 Estrutura do Projeto

```bash
SecureTaskManager/
│
├── SecureTaskManager.API          # API .NET com autenticação e gerenciamento de usuários
│   ├── Controllers
│   ├── Data
│   ├── Models
│   ├── Repositories
│   ├── Services
│   └── Settings
│
├── SecureTaskManager.Client       # Projeto Blazor WebAssembly (Front-end)
│   ├── Pages
│   ├── Shared
│   ├── Services
│   └── Auth
│
├── SecureTaskManager.Shared       # DTOs e modelos compartilhados
│
└── README.md

````
## 🚀 Como Rodar o Projeto

### Pré-requisitos

- .NET SDK 8.0+
- Visual Studio 2022+ ou VS Code

### Passo a passo

1. Clone o repositório:

```bash
git clone https://github.com/SEU_USUARIO/SecureTaskManager.git
cd SecureTaskManager
```

2. Execute o seguinte comando para aplicar as migrações e criar o banco de dados SQLite:
```bash
cd SecureTaskManager.API
dotnet ef database update
```
Isso criará um arquivo .db (banco SQLite) automaticamente na pasta do projeto.

3. Execute a API:
```bash
dotnet run
```
4. Execute o projeto Blazor WebAssembly:
```bash
cd ../SecureTaskManager.Client
dotnet run
```

🌱 Seed do Usuário Administrador Master
Para facilitar os testes administrativos, o projeto possui um perfil de execução especial que cria automaticamente um usuário com permissões de administrador, caso ele ainda não exista.

👨‍💼 Execute o seed com:

```bash
dotnet run --launch-profile SeedMaster
```

Esse comando:

- Cria um usuário com:

   - Email: admin@master.com

   - Senha: SenhaForte123@

   - Role: Admin

- É executado apenas se o usuário ainda não estiver presente no banco de dados.

- Usa o ambiente Development.

⚠️ Importante: Não execute esse comando em produção. Ele é destinado apenas para desenvolvimento e testes locais.


## 🧪 Testes

Os testes de integração estão sendo implementados com:

- ✅ xUnit
- ✅ WebApplicationFactory
- ✅ FluentAssertions



## 🛡️ Segurança

- Tokens JWT protegidos e armazenados com segurança
- Hash de senhas com Identity
- Proteção de rotas por role
- Políticas de autorização


## 📌 Funcionalidades

- ✅ Cadastro de usuário
- ✅ Login com JWT
- ✅ Visualização do próprio perfil
- ✅ Edição de perfil
- ✅ Alteração de senha
- ✅ Autoexclusão de conta
- ✅ Exclusão de usuários (Admin)
- ✅ Listagem de usuários (Admin)

## 💻 Telas (em desenvolvimento)

- ⚙️ Interface Blazor WebAssembly com autenticação
- 📄 Página de Login
- 👤 Página de Perfil
- 🛠️ Tela de Gerenciamento (Admin)


🧑‍💻 Autor
JRALVESLIMA – Desenvolvedor em transição de carreira, apaixonado por tecnologia e aprendizado contínuo.

[LinkedIn](https://www.linkedin.com/in/-junior-a-lima) | [GitHub](https://github.com/JRALVESLIMA) 


