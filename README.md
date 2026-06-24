# OpenFinancialExchange

**OpenFinancialExchange** é uma API backend para importação, armazenamento e consulta de extratos bancários no formato OFX (Open Financial Exchange), desenvolvida com Clean Architecture, CQRS e DDD em .NET 9.

---

## Índice

- [Visão Geral](#visão-geral)
- [Funcionalidades](#funcionalidades)
- [Stack Tecnológica](#stack-tecnológica)
- [Arquitetura](#arquitetura)
- [Camadas de Teste](#camadas-de-teste)
- [Modelagem de Dados](#modelagem-de-dados)
- [Estrutura de Pastas](#estrutura-de-pastas)
- [Padrões e Princípios](#padrões-e-princípios)
- [API — Endpoints](#api--endpoints)
- [Dados de Seed](#dados-de-seed)
- [Como Executar](#como-executar)
- [Decisões Arquiteturais](#decisões-arquiteturais)

---

## Visão Geral

O **OpenFinancialExchange** recebe arquivos OFX exportados por bancos brasileiros (Bradesco, Banco do Brasil e outros) e os armazena de forma estruturada em SQL Server. A API permite consultar instituições financeiras, contas bancárias, importações, extratos e transações com deduplicação automática via SHA-256 e suporte a particularidades do formato OFX de cada banco.

---

## Funcionalidades

### Gestão de Instituições e Contas

- **Instituições Financeiras**: cadastro com `BankId` (ex: `"237"` Bradesco, `"001"` BB), `OrgName` nullable (Bradesco não inclui no OFX) e `Fid` opcional.
- **Contas Bancárias**: vinculadas à instituição, com `AcctType` validado (`CHECKING`, `SAVINGS`, `MONEYMRKT`, `CREDITLINE`, `CD`, `OTHER`), `BranchId` nullable (Bradesco não inclui agência no OFX).
- **Soft delete**: desativação lógica (`IsActive = 0`) — nunca exclusão física.

### Importação OFX

- **Deduplicação por SHA-256**: `FileHash` de 64 caracteres garante que o mesmo arquivo não seja importado duas vezes.
- **Metadados do cabeçalho OFX**: versões de header, charset, encoding, security, compression, `OldFileUid`/`NewFileUid`.
- **Armazenamento do XML/SGML original**: campo `OfxData NVARCHAR(MAX)` preserva o conteúdo bruto.

### Extratos e Transações

- **Statements OFX**: múltiplos extratos por importação, com período (`DtStart`/`DtEnd`), moeda (`CurDef`), status do servidor e `TrnUid`.
- **Transações**: `TrnType` validado (DEBIT, CREDIT, ATM, POS, XFER, etc.), `DtPosted` em `DATETIME2`, valor em `DECIMAL(18,2)`, `FitId` nullable (BB pode não ter).
- **Saldos**: `LEDGER` e `AVAILABLE` por extrato.
- **Índice filtrado único**: `FitId` único por statement quando `NOT NULL` — resolve duplicatas específicas do BB.

### Consultas

- Transações por extrato ou por conta bancária (com filtro opcional de período).
- Views SQL: `VwOfxTransactionsFull` (JOIN completo) e `VwSaldoAtualPorConta` (saldo mais recente por conta).

---

## Stack Tecnológica

### Backend

| Tecnologia | Versão | Uso |
|---|---|---|
| **.NET** | 9.0 | Plataforma principal |
| **C#** | 13 | Linguagem |
| **ASP.NET Core Web API** | 9.0 | Framework HTTP |
| **Entity Framework Core** | 9.0.5 | ORM (SQL Server) |
| **SQL Server** | 2019+ | Banco de dados |
| **MediatR** | 12.4.1 | Mediador CQRS |
| **FluentValidation** | 11.11.0 | Validação de commands |
| **BCrypt.Net-Next** | 4.0.3 | Hash de senhas |
| **JWT Bearer** | 9.0 | Autenticação |
| **Swashbuckle (Swagger)** | — | Documentação da API |

### Testes Backend

| Tecnologia | Uso |
|---|---|
| **xUnit** | Framework de testes |
| **FluentAssertions** | Asserções expressivas |
| **NSubstitute** | Mocks e stubs (unit tests) |
| **Moq** | Mocks e stubs (BDD specs) |
| **Reqnroll** | BDD / Gherkin (specs) |
| **NetArchTest.Rules** | Testes de arquitetura DDD |

---

## Arquitetura

Clean Architecture com 4 camadas. Dependências fluem de fora para dentro — **nunca** o contrário.

```
┌─────────────────────────────────────────────┐
│                    API                      │  ← Controllers, JWT, Swagger
├─────────────────────────────────────────────┤
│              Infrastructure                 │  ← EF Core, Repositórios, BCrypt
├─────────────────────────────────────────────┤
│               Application                  │  ← CQRS, Validação (MediatR + FluentValidation)
├─────────────────────────────────────────────┤
│                 Domain                      │  ← Entidades, Primitivos, Interfaces
└─────────────────────────────────────────────┘
```

> **Domain tem zero dependências externas** — sem MediatR, EF Core ou FluentValidation.

### Fluxo de uma Requisição

```
HTTP Request
    │
    ▼
Controller (API)          [Authorize] — JWT Bearer
    │  new Command / Query
    ▼
MediatR Pipeline
    └── ValidationBehavior (FluentValidation automático)
         │
         ▼
    CommandHandler / QueryHandler (Application — internal sealed)
         │  Result<T> / Result
         ▼
    Repository (Infrastructure — internal sealed, AsNoTracking em leituras)
         │
         ▼
    SQL Server (OpenFinancialExchangeDb)
```

---

## Camadas de Teste

O projeto possui **três projetos de teste** complementares:

### 1. OpenFinancialExchange.Tests — Testes Unitários

Testes unitários com xUnit + FluentAssertions + NSubstitute cobrindo Domain e Application:

- **Domain:** `FinancialInstitution`, `BankAccount`, `Result`/`Error` — criação, validações, soft delete.
- **Application:** handlers de command e query com repositórios mockados via NSubstitute.

```bash
dotnet test OpenFinancialExchange.Tests --verbosity normal
```

### 2. OpenFinancialExchange.Specs — BDD / Gherkin

Especificações de comportamento com Reqnroll + Moq, cobrindo cenários de negócio via arquivos `.feature`:

| Feature | Cenários |
|---|---|
| Financial Institution Management | Create válido, duplicata, BankId vazio |

```bash
dotnet test OpenFinancialExchange.Specs --verbosity normal
```

### 3. OpenFinancialExchange.ArchTests — Testes de Arquitetura (31 testes)

Testes de arquitetura com **NetArchTest.Rules 1.3.2** garantindo que as fronteiras de Clean Architecture/DDD nunca sejam violadas:

| Regras Verificadas | Quantidade |
|---|---|
| Isolamento de camadas (Domain/Application/Infrastructure sem dependências proibidas) | 6 |
| Domain não referencia MediatR, EF Core, FluentValidation, API | 4 |
| Application não referencia EF Core nem API | 2 |
| Handlers são `internal sealed` | 2 |
| Repositórios concretos são `internal sealed` | 1 |
| Commands/Queries/Validators residem em `Features` | 3 |
| Aggregates herdam `AggregateRoot` | 4 |
| EF Configurations são `internal sealed` e implementam `IEntityTypeConfiguration<T>` | 2 |
| Response DTOs são `sealed` | 1 |
| Entidades residem em `Domain.Entities` | 1 |
| Interfaces de repositório residem em `Domain.Repositories` | 1 |
| Repositórios concretos residem em `Persistence.Repositories` | 1 |
| Configurations residem em `Persistence.Configurations` | 1 |
| Validators herdam `AbstractValidator<T>` | 1 |
| `IUnitOfWork` reside em `Domain.Repositories` | 1 |

```bash
dotnet test OpenFinancialExchange.ArchTests --verbosity normal
# Total: 31 | Passed: 31
```

---

## Modelagem de Dados

### Hierarquia das Tabelas

```
FinancialInstitutions  (Id PK BIGINT IDENTITY)
  └── BankAccounts     (Id PK, FinancialInstitutionId FK → Restrict)
        └── OfxStatements  (Id PK, BankAccountId FK → Restrict)
              ├── OfxTransactions  (Id PK, StatementId FK → Cascade)
              └── OfxBalances      (Id PK, StatementId FK → Cascade)

OfxImports             (Id PK — arquivo OFX processado, FileHash SHA-256 UNIQUE)
  └── OfxStatements    (ImportId FK → Cascade — uma importação pode gerar vários statements)
```

### Tabelas

```sql
-- FinancialInstitutions
Id          BIGINT IDENTITY(1,1) PK
BankId      NVARCHAR(20) NOT NULL         -- "237" (Bradesco), "001" (BB)
OrgName     NVARCHAR(100) NULL            -- NULL para Bradesco
Fid         NVARCHAR(50) NULL
IsActive    BIT DEFAULT 1 NOT NULL
CreatedAt   DATETIME2 NOT NULL
UpdatedAt   DATETIME2 NOT NULL

-- BankAccounts
Id                     BIGINT IDENTITY(1,1) PK
FinancialInstitutionId BIGINT FK NOT NULL
BankId                 NVARCHAR(20) NOT NULL
BranchId               NVARCHAR(20) NULL   -- NULL para Bradesco
AcctId                 NVARCHAR(50) NOT NULL
AcctType               NVARCHAR(20) NOT NULL  -- CHECKING | SAVINGS | MONEYMRKT | CREDITLINE | CD | OTHER
IsActive               BIT DEFAULT 1 NOT NULL
CreatedAt / UpdatedAt  DATETIME2

-- OfxImports
Id               BIGINT IDENTITY(1,1) PK
FileName         NVARCHAR(255) NOT NULL
FileHash         NVARCHAR(64) NOT NULL UNIQUE   -- SHA-256 hex (64 chars) para deduplicação
OfxData          NVARCHAR(MAX) NULL             -- conteúdo OFX bruto
OfxHeaderVersion SMALLINT NULL
OfxVersion       SMALLINT NULL
Encoding         NVARCHAR(20) NULL
Charset          NVARCHAR(20) NULL
Security         NVARCHAR(20) NULL
Compression      NVARCHAR(20) NULL
OldFileUid       NVARCHAR(50) NULL
NewFileUid       NVARCHAR(50) NULL
ImportedAt       DATETIME2 NOT NULL

-- OfxStatements
Id             BIGINT IDENTITY(1,1) PK
ImportId       BIGINT FK NOT NULL
BankAccountId  BIGINT FK NOT NULL
TrnUid         NVARCHAR(36) NULL
CurDef         NVARCHAR(3) NOT NULL     -- ex: "BRL"
DtServer       DATETIME2 NULL
Language       NVARCHAR(10) NULL
StatusCode     SMALLINT NULL
StatusSeverity NVARCHAR(10) NULL
DtStart        DATETIME2 NULL
DtEnd          DATETIME2 NULL
CreatedAt      DATETIME2 NOT NULL

-- OfxTransactions
Id          BIGINT IDENTITY(1,1) PK
StatementId BIGINT FK NOT NULL
TrnType     NVARCHAR(20) NOT NULL       -- DEBIT | CREDIT | INT | ATM | POS | XFER | OTHER …
DtPosted    DATETIME2 NOT NULL
TrnAmt      DECIMAL(18,2) NOT NULL
FitId       NVARCHAR(255) NULL          -- índice filtrado UNIQUE quando NOT NULL
Name        NVARCHAR(100) NULL
Memo        NVARCHAR(255) NULL
CheckNum    NVARCHAR(20) NULL
CreatedAt   DATETIME2 NOT NULL

-- OfxBalances
Id          BIGINT IDENTITY(1,1) PK
StatementId BIGINT FK NOT NULL
BalanceType NVARCHAR(20) NOT NULL       -- LEDGER | AVAILABLE
BalAmt      DECIMAL(18,2) NOT NULL
DtAsOf      DATETIME2 NOT NULL
CreatedAt   DATETIME2 NOT NULL
```

### Índices Críticos

```sql
-- FitId único POR statement (Bradesco repete FitId entre statements distintos)
CREATE UNIQUE INDEX UIX_OfxTransactions_StatementId_FitId
    ON OfxTransactions (StatementId, FitId)
    WHERE FitId IS NOT NULL;            -- índice FILTRADO — essencial para BB (FitId nullable)

-- Cobertura para queries por período e tipo
CREATE INDEX IX_OfxTransactions_DtPosted_TrnType
    ON OfxTransactions (DtPosted, TrnType)
    INCLUDE (TrnAmt, Memo, Name);

-- Unicidade de conta
CREATE UNIQUE INDEX UQ_BankAccounts_BankId_BranchId_AcctId
    ON BankAccounts (BankId, BranchId, AcctId);

-- Unicidade de instituição
CREATE UNIQUE INDEX UQ_FinancialInstitutions_BankId_Fid
    ON FinancialInstitutions (BankId, Fid);
```

### Views SQL

| View | Descrição |
|---|---|
| `VwOfxTransactionsFull` | JOIN completo: Transações → Statements → Contas → Instituições |
| `VwSaldoAtualPorConta` | Saldo mais recente (`DtAsOf MAX`) por `BankAccountId` |

### Entidades C# × Banco

| Entidade C# | Tipo | Aggregate Root? |
|---|---|---|
| `FinancialInstitution` | `AggregateRoot` | ✅ |
| `BankAccount` | `AggregateRoot` | ✅ |
| `OfxImport` | `AggregateRoot` | ✅ |
| `OfxStatement` | `AggregateRoot` | ✅ (possui `_transactions` e `_balances`) |
| `OfxTransaction` | `Entity` | — |
| `OfxBalance` | `Entity` | — |

---

## Estrutura de Pastas

```
OpenFinancialExchange/
│
├── Sql/
│   ├── ofx_database_model_sqlserver.sql   ← DDL: 6 tabelas, 2 views, índices, FKs
│   └── ofx_seed_data.sql                  ← 1.431 transações reais (Bradesco + BB)
│
└── BackEnd/
    └── OpenFinancialExchange/
        │
        ├── OpenFinancialExchange.Domain/
        │   ├── Entities/
        │   │   ├── FinancialInstitution.cs    ← AggregateRoot | Create, UpdateDetails, Deactivate
        │   │   ├── BankAccount.cs             ← AggregateRoot | ValidAcctTypes HashSet
        │   │   ├── OfxImport.cs               ← AggregateRoot | FileHash SHA-256
        │   │   ├── OfxStatement.cs            ← AggregateRoot | _transactions, _balances (private)
        │   │   ├── OfxTransaction.cs          ← Entity | ValidTrnTypes HashSet
        │   │   └── OfxBalance.cs              ← Entity | ValidTypes: LEDGER, AVAILABLE
        │   ├── Primitives/
        │   │   ├── Entity.cs                  ← Id: long, IEquatable<Entity>
        │   │   ├── AggregateRoot.cs           ← Domain Events (IDomainEvent)
        │   │   ├── IDomainEvent.cs            ← marcador puro — sem MediatR
        │   │   ├── Error.cs                   ← sealed record (Code, Message)
        │   │   ├── Result.cs                  ← Result + Result<TValue> : Result
        │   │   └── ValueObject.cs             ← GetAtomicValues(), IEquatable
        │   └── Repositories/
        │       ├── IFinancialInstitutionRepository.cs
        │       ├── IBankAccountRepository.cs
        │       ├── IOfxImportRepository.cs        ← ExistsByHashAsync (SHA-256)
        │       ├── IOfxStatementRepository.cs
        │       ├── IOfxTransactionRepository.cs   ← GetByBankAccountAsync (from/to)
        │       └── IUnitOfWork.cs
        │
        ├── OpenFinancialExchange.Application/
        │   ├── Abstractions/
        │   │   ├── DependencyInjection.cs         ← AddApplication()
        │   │   └── Messaging/
        │   │       ├── ICommand.cs                ← ICommand / ICommand<TResponse>
        │   │       ├── ICommandHandler.cs
        │   │       ├── IQuery.cs
        │   │       └── IQueryHandler.cs
        │   └── Features/
        │       ├── FinancialInstitutions/          ← Create, Update, GetAll, GetById
        │       ├── BankAccounts/                   ← Create, Update, GetAll, GetById
        │       ├── OfxImports/                     ← Create, GetAll, GetById
        │       ├── OfxStatements/                  ← GetAll, GetById (read-only)
        │       └── OfxTransactions/                ← GetByStatement, GetByBankAccount (read-only)
        │
        ├── OpenFinancialExchange.Infrastructure/
        │   ├── DependencyInjection.cs              ← AddInfrastructure() — DbContext + Repositórios
        │   └── Persistence/
        │       ├── AppDbContext.cs                 ← sealed, IUnitOfWork, ApplyConfigurationsFromAssembly
        │       ├── Configurations/                 ← internal sealed : IEntityTypeConfiguration<T>
        │       │   ├── FinancialInstitutionConfiguration.cs
        │       │   ├── BankAccountConfiguration.cs
        │       │   ├── OfxImportConfiguration.cs
        │       │   ├── OfxStatementConfiguration.cs   ← HasMany + UsePropertyAccessMode(Field)
        │       │   ├── OfxTransactionConfiguration.cs ← índice filtrado UIX + índice cobertura IX
        │       │   └── OfxBalanceConfiguration.cs
        │       └── Repositories/                   ← internal sealed, AsNoTracking em leituras
        │           ├── FinancialInstitutionRepository.cs
        │           ├── BankAccountRepository.cs
        │           ├── OfxImportRepository.cs
        │           ├── OfxStatementRepository.cs
        │           └── OfxTransactionRepository.cs
        │
        ├── OpenFinancialExchange.API/
        │   ├── Controllers/
        │   │   ├── ApiController.cs               ← abstract, HandleFailure() (404/409/400)
        │   │   ├── FinancialInstitutionsController.cs  ← sealed, [Authorize], {id:long}
        │   │   ├── BankAccountsController.cs
        │   │   ├── OfxImportsController.cs
        │   │   ├── OfxStatementsController.cs
        │   │   └── OfxTransactionsController.cs
        │   └── Program.cs                         ← JWT Bearer, Swagger + Bearer, FluentValidation auto
        │
        ├── OpenFinancialExchange.Tests/            ← xUnit + FluentAssertions + NSubstitute
        │   ├── Domain/
        │   │   ├── FinancialInstitutionTests.cs
        │   │   ├── BankAccountTests.cs
        │   │   └── ResultTests.cs
        │   └── Application/
        │       ├── CreateFinancialInstitutionHandlerTests.cs
        │       └── GetAllFinancialInstitutionsHandlerTests.cs
        │
        ├── OpenFinancialExchange.Specs/            ← Reqnroll + Moq
        │   ├── Features/
        │   │   └── FinancialInstitution.feature
        │   └── StepDefinitions/
        │       └── FinancialInstitutionSteps.cs
        │
        └── OpenFinancialExchange.ArchTests/        ← NetArchTest.Rules (31 testes)
            └── ArchitectureTests.cs
```

---

## Padrões e Princípios

### Domain-Driven Design (DDD)

- **Aggregate Roots**: `FinancialInstitution`, `BankAccount`, `OfxImport`, `OfxStatement`.
- **Entities**: `OfxTransaction`, `OfxBalance`.
- **Rich Domain Model**: propriedades com `private set`; estado alterado apenas pelos métodos da entidade (`Create`, `UpdateDetails`, `Deactivate`).
- **Inversão de Dependência**: interfaces no Domain, implementações na Infrastructure.
- **Domain sem dependências externas**: sem MediatR, EF Core ou FluentValidation no projeto Domain.

### CQRS com MediatR

```
Command → altera estado   → Result<long> (Create) / Result (Update)
Query   → lê dados        → Result<TResponse>
```

Cada caso de uso tem pasta isolada: `Command/Query + Handler + Validator`. Handlers são sempre `internal sealed class`.

### Result Pattern

```csharp
// Sem exceções para controle de fluxo de negócio
return Result.Failure<FinancialInstitution>(
    new Error("FinancialInstitution.EmptyBankId", "Bank ID is required."));
return Result.Success(entity.Id);

// No controller: HandleFailure() mapeia automaticamente
// .NotFound  → 404
// .AlreadyExists / .Duplicate → 409
// default    → 400
```

### EF Core 9 — Boas Práticas

- `AsNoTracking()` em **todos** os métodos de leitura dos repositórios.
- `datetime2` para todas as colunas de data — nunca `datetime`.
- `decimal(18,2)` para valores monetários.
- `UseIdentityColumn()` em todas as PKs BIGINT.
- `OnDelete(Restrict)` entre aggregates distintos; `OnDelete(Cascade)` para coleções próprias.
- `UsePropertyAccessMode(PropertyAccessMode.Field)` nas coleções privadas do `OfxStatement`.

### Especificidades OFX Suportadas

| Particularidade | Solução |
|---|---|
| Bradesco não inclui `OrgName` no OFX | `OrgName NVARCHAR(100) NULL` |
| Bradesco não inclui `BranchId` | `BranchId NVARCHAR(20) NULL` |
| BB pode ter `FitId = NULL` | Índice filtrado `WHERE FitId IS NOT NULL` |
| Bradesco repete `FitId` entre statements | Índice único por `(StatementId, FitId)`, não global |
| Datas com ano < 1753 (inválido para SQL Server) | Armazenadas como `NULL` |
| Deduplicação de arquivos | SHA-256 (`FileHash NVARCHAR(64) UNIQUE`) |

### Repository + Unit of Work

- `IUnitOfWork.CommitAsync()` persiste atomicamente ao final de cada command handler.
- Soft delete: `entity.Deactivate()` → `IsActive = false` + `UpdatedAt = UtcNow`.
- Nunca DELETE físico.

### Testes de Arquitetura (NetArchTest)

Fronteiras DDD verificadas automaticamente. Nenhuma violação pode passar despercebida:

```
Domain      → não pode depender de Application / Infrastructure / API / MediatR / EF Core / FluentValidation
Application → não pode depender de Infrastructure / API / EF Core
Infrastructure → não pode depender de API
Handlers    → internal sealed, terminam em "Handler"
Repositórios concretos → internal sealed, terminam em "Repository", em Persistence.Repositories
Configurations → internal sealed, implementam IEntityTypeConfiguration<T>, em Persistence.Configurations
Response DTOs → sealed records, terminam em "Response"
```

---

## API — Endpoints

Todos os endpoints exigem `[Authorize]` (JWT Bearer). Autenticação configurada via `appsettings.json`.

### Instituições Financeiras

```
GET  /api/financialinstitutions           → Lista todas as ativas
GET  /api/financialinstitutions/{id}      → Busca por Id (long)
POST /api/financialinstitutions           → 201 Created com { id }
PUT  /api/financialinstitutions/{id}      → 204 No Content
```

### Contas Bancárias

```
GET  /api/bankaccounts                    → Lista todas as ativas
GET  /api/bankaccounts/{id}              → Busca por Id (long)
POST /api/bankaccounts                    → 201 Created com { id }
PUT  /api/bankaccounts/{id}              → 204 No Content
```

### Importações OFX

```
GET  /api/ofximports                      → Lista todas as importações
GET  /api/ofximports/{id}                → Busca por Id (long)
POST /api/ofximports                      → 201 Created com { id }
```

### Extratos OFX (somente leitura)

```
GET  /api/ofxstatements                   → Lista todos os extratos
GET  /api/ofxstatements/{id}             → Busca por Id (long)
```

### Transações OFX (somente leitura)

```
GET  /api/ofxtransactions/by-statement/{statementId}
GET  /api/ofxtransactions/by-account/{bankAccountId}
```

### Respostas de Erro Padronizadas

| Status | Situação |
|---|---|
| 400 | Regra de negócio violada ou validação FluentValidation |
| 401 | Não autenticado (token ausente ou inválido) |
| 404 | Recurso não encontrado |
| 409 | Duplicata detectada (mesma instituição, conta ou hash de arquivo) |

---

## Dados de Seed

| Arquivo | Conteúdo |
|---|---|
| `ofx_database_model_sqlserver.sql` | DDL completo — 6 tabelas, 2 views, todos os índices e FKs |
| `ofx_seed_data.sql` | **1.431 transações reais** extraídas de 14 arquivos OFX: Bradesco (conta corrente PJ, múltiplos períodos) e Banco do Brasil (conta corrente PF) |

### Ordem de execução

```sql
-- 1. Criar estrutura (apenas na primeira vez ou após drop do banco)
Sql/ofx_database_model_sqlserver.sql

-- 2. Popular com dados reais
Sql/ofx_seed_data.sql
```

---

## Como Executar

### Pré-requisitos

| Ferramenta | Versão mínima |
|---|---|
| .NET SDK | 9.0 |
| SQL Server | 2019+ (ou Express / LocalDB) |

### 1. Criar o banco

Execute os scripts SQL na ordem descrita acima.

### 2. Configurar `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=OpenFinancialExchangeDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Secret": "SUA_CHAVE_SECRETA_COM_NO_MINIMO_32_CARACTERES",
    "Issuer": "OpenFinancialExchange.API",
    "Audience": "OpenFinancialExchange.Client",
    "ExpiresInMinutes": 60
  }
}
```

### 3. Rodar a API

```bash
cd BackEnd/OpenFinancialExchange
dotnet run --project OpenFinancialExchange.API
```

O Swagger UI sobe em `https://localhost:{porta}/swagger`. Clique em **Authorize** e informe o token JWT Bearer.

### 4. Executar os testes

```bash
cd BackEnd/OpenFinancialExchange

# Todos os projetos de teste
dotnet test

# Por projeto
dotnet test OpenFinancialExchange.Tests      # Testes unitários
dotnet test OpenFinancialExchange.Specs      # Specs BDD / Gherkin
dotnet test OpenFinancialExchange.ArchTests  # 31 testes de arquitetura
```

---

## Decisões Arquiteturais

| Decisão | Justificativa |
|---|---|
| **Clean Architecture** | Domain sem dependências externas — regras de negócio OFX isoladas e testáveis |
| **CQRS com MediatR** | Commands (escrita) e Queries (leitura) claramente separados; pipeline behavior centraliza validação |
| **Result Pattern** | Erros de negócio explícitos no tipo de retorno; sem `try/catch` espalhado pelos handlers |
| **Índice filtrado `WHERE FitId IS NOT NULL`** | BB pode ter `FitId NULL`; índice padrão `UNIQUE` rejeitaria múltiplos NULLs |
| **Índice único por `(StatementId, FitId)`** | Bradesco reutiliza o mesmo `FitId` em statements distintos — unicidade global causaria rejeições incorretas |
| **`FileHash` SHA-256 (64 chars)** | Deduplicação de importação confiável sem processar o arquivo OFX novamente |
| **`OrgName` e `BranchId` nullable** | Bradesco não inclui esses campos no OFX — rejeitar seria incompatível com dados reais |
| **`DtPosted` como `DATETIME2` não nullable** | Datas inválidas (ano < 1753) são filtradas no parser antes de chegar ao banco |
| **`AsNoTracking()` em todas as leituras** | Queries de leitura não precisam de rastreamento; melhora performance em extratos com muitas transações |
| **`OnDelete(Restrict)` entre aggregates** | FKs entre aggregates distintos nunca fazem cascade delete automático |
| **`OnDelete(Cascade)` em coleções próprias** | `OfxStatement → OfxTransactions/OfxBalances` — filho pertence ao aggregate |
| **`UsePropertyAccessMode(Field)` em `OfxStatement`** | EF Core popula `_transactions` e `_balances` (campos privados) corretamente ao incluir as coleções via `Include()` |
| **`internal sealed` em handlers e repositórios** | Encapsulamento por design — classes de implementação não devem ser expostas fora de suas camadas |
| **Soft delete universal** | `IsActive = 0`; dados históricos preservados — nunca DELETE físico |
| **PKs como `BIGINT IDENTITY`** | Suporte a volumes altos de transações OFX sem risco de overflow de `INT` |
| **`DECIMAL(18,2)` para valores monetários** | Precisão financeira garantida; evita erros de ponto flutuante |
| **NetArchTest.Rules para arquitetura** | Fronteiras DDD verificadas automaticamente — nenhum PR pode quebrar a isolação de camadas |

---

*Projeto desenvolvido com foco em qualidade de código, aderência às melhores práticas de Clean Architecture / DDD e suporte real aos formatos OFX dos principais bancos brasileiros.*
