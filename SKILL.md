# OpenFinancialExchange — Padrões de Arquitetura e Desenvolvimento

## Propósito

Você é um arquiteto fullstack especialista neste projeto. Quando ativado, aplique consistentemente todos os padrões documentados aqui ao criar features, revisar código ou fazer scaffold. **Nunca altere features já homologadas** — apenas adicione novas seguindo os mesmos padrões.

---

## Stack Tecnológico

| Camada | Tecnologia | Versão |
|---|---|---|
| Runtime | .NET 9 / C# 13 | 9.0 |
| Web API | ASP.NET Core Web API | 9.0 |
| ORM | EF Core SQL Server | 9.0.5 |
| Mensageria | MediatR | 12.4.1 |
| Validação | FluentValidation | 11.11.0 |
| Auth | JWT Bearer + BCrypt (workFactor 12) | 9.0.5 / 4.0.3 |
| Testes unitários | xUnit + FluentAssertions + NSubstitute | 2.9.2 / 6.12.2 / 5.3.0 |
| Testes BDD | Reqnroll (Gherkin) + Moq | 2.4.1 / 4.20.72 |
| Testes arquitetura | NetArchTest.Rules | 1.3.2 |
| Banco de dados | SQL Server (T-SQL, PascalCase) | — |

---

## 1. Estrutura da Solução

```
OpenFinancialExchange/
├── Sql/
│   ├── ofx_database_model_sqlserver.sql   # DDL completo — 6 tabelas + 2 views + índices
│   └── ofx_seed_data.sql                  # 1431 transações reais Bradesco + BB
├── BackEnd/
│   └── OpenFinancialExchange/
│       ├── OpenFinancialExchange.Domain           # Zero dependências externas
│       ├── OpenFinancialExchange.Application      # MediatR + FluentValidation
│       ├── OpenFinancialExchange.Infrastructure   # EF Core + Repositórios
│       ├── OpenFinancialExchange.API              # JWT + Swagger + Controllers
│       ├── OpenFinancialExchange.Tests            # xUnit + FluentAssertions + NSubstitute
│       ├── OpenFinancialExchange.Specs            # Reqnroll + Moq
│       └── OpenFinancialExchange.ArchTests        # NetArchTest (31 testes)
```

**Regra de dependência (Clean Architecture):**
```
API → Application → Domain
Infrastructure implementa interfaces do Domain
```
Domain: **zero** dependências externas (sem MediatR, EF Core, FluentValidation).

---

## 2. Banco de Dados

### 2.1 Esquema — 6 Tabelas

```
FinancialInstitutions  (Id PK BIGINT IDENTITY)
  └── BankAccounts     (Id PK, FinancialInstitutionId FK)
        └── OfxStatements  (Id PK, BankAccountId FK, ImportId FK)
              ├── OfxTransactions  (Id PK, StatementId FK)
              └── OfxBalances      (Id PK, StatementId FK)

OfxImports             (Id PK — arquivo OFX processado, hash SHA-256)
  └── OfxStatements    (ImportId FK — uma importação pode gerar vários statements)
```

### 2.2 Tabelas e Campos Relevantes

```sql
-- FinancialInstitutions
Id          BIGINT IDENTITY(1,1) PK
BankId      NVARCHAR(20) NOT NULL  -- ex: "237" (Bradesco), "001" (BB)
OrgName     NVARCHAR(100) NULL     -- NULL para Bradesco (não presente no OFX)
Fid         NVARCHAR(50) NULL
IsActive    BIT DEFAULT 1 NOT NULL
CreatedAt   DATETIME2 NOT NULL
UpdatedAt   DATETIME2 NOT NULL

-- BankAccounts
Id                    BIGINT IDENTITY(1,1) PK
FinancialInstitutionId BIGINT FK NOT NULL
BankId                NVARCHAR(20) NOT NULL
BranchId              NVARCHAR(20) NULL    -- NULL para Bradesco
AcctId                NVARCHAR(50) NOT NULL
AcctType              NVARCHAR(20) NOT NULL -- CHECKING | SAVINGS | MONEYMRKT | CREDITLINE | CD | OTHER
IsActive              BIT DEFAULT 1 NOT NULL
CreatedAt/UpdatedAt   DATETIME2

-- OfxImports
Id               BIGINT IDENTITY(1,1) PK
FileName         NVARCHAR(255) NOT NULL
FileHash         NVARCHAR(64) NOT NULL UNIQUE   -- SHA-256 hex para deduplicação
OfxHeaderVersion SMALLINT NULL
OfxVersion       SMALLINT NULL
OfxData          NVARCHAR(MAX) NULL
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
CurDef         NVARCHAR(3) NOT NULL  -- ex: "BRL"
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
TrnType     NVARCHAR(20) NOT NULL  -- DEBIT | CREDIT | INT | ATM | POS | XFER | OTHER …
DtPosted    DATETIME2 NOT NULL
TrnAmt      DECIMAL(18,2) NOT NULL
FitId       NVARCHAR(255) NULL     -- índice filtrado UNIQUE quando NOT NULL
Name        NVARCHAR(100) NULL
Memo        NVARCHAR(255) NULL
CheckNum    NVARCHAR(20) NULL
CreatedAt   DATETIME2 NOT NULL

-- OfxBalances
Id          BIGINT IDENTITY(1,1) PK
StatementId BIGINT FK NOT NULL
BalanceType NVARCHAR(20) NOT NULL  -- LEDGER | AVAILABLE
BalAmt      DECIMAL(18,2) NOT NULL
DtAsOf      DATETIME2 NOT NULL
CreatedAt   DATETIME2 NOT NULL
```

### 2.3 Índices Críticos

```sql
-- Transações: FitId único POR statement (não globalmente — Bradesco repete FitId entre statements)
CREATE UNIQUE INDEX UIX_OfxTransactions_StatementId_FitId
    ON OfxTransactions (StatementId, FitId)
    WHERE FitId IS NOT NULL;          -- índice FILTRADO — essencial para BB que tem FitId NULL

-- Cobertura para queries por período
CREATE INDEX IX_OfxTransactions_DtPosted_TrnType
    ON OfxTransactions (DtPosted, TrnType)
    INCLUDE (TrnAmt, Memo, Name);

-- BankAccounts: unicidade por conta
CREATE UNIQUE INDEX UQ_BankAccounts_BankId_BranchId_AcctId
    ON BankAccounts (BankId, BranchId, AcctId);
```

### 2.4 Views

```sql
VwOfxTransactionsFull    -- JOIN OfxTransactions → OfxStatements → BankAccounts → FinancialInstitutions
VwSaldoAtualPorConta     -- saldo mais recente (DtAsOf MAX) por BankAccountId
```

### 2.5 Regras SQL

- **PascalCase** em tudo: tabelas, colunas, índices, FKs.
- **BIGINT IDENTITY(1,1)** para todas as PKs.
- **DATETIME2** para todas as datas (nunca DATETIME).
- **DECIMAL(18,2)** para valores monetários.
- **CONVERT(DATETIME2, 'yyyy-mm-ddThh:mm:ss', 126)** para datas em seeds/scripts.
- **Soft delete:** nunca DELETE físico — sempre `UPDATE IsActive = 0`.
- **Datas inválidas** (ano < 1753 — limite do SQL Server datetime): gravar como NULL.
- **`SET IDENTITY_INSERT <tabela> ON`** com `BEGIN TRY SET IDENTITY_INSERT <tabela> OFF END TRY BEGIN CATCH END CATCH` antes de cada bloco de inserção controlada.
- **`DBCC CHECKIDENT ('tabela', RESEED, 0)`** em scripts de reset/seed.

### 2.6 Entidades C# × Banco

| Entidade C# | Tipo | Aggregate Root? |
|---|---|---|
| `FinancialInstitution` | `AggregateRoot` | ✅ |
| `BankAccount` | `AggregateRoot` | ✅ |
| `OfxImport` | `AggregateRoot` | ✅ |
| `OfxStatement` | `AggregateRoot` | ✅ |
| `OfxTransaction` | `Entity` | — |
| `OfxBalance` | `Entity` | — |

---

## 3. Domain Layer

### 3.1 Classes Base (Primitivos)

```csharp
// Entity — Id como long (BIGINT)
public abstract class Entity : IEquatable<Entity>
{
    public long Id { get; protected set; }
    protected Entity(long id) => Id = id;
    // Equals/GetHashCode/== por Id e tipo
}

// AggregateRoot — possui Domain Events
public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];
    protected AggregateRoot(long id) : base(id) { }
    public IReadOnlyList<IDomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}

// IDomainEvent — marcador puro SEM MediatR no Domain
public interface IDomainEvent { }

// ValueObject
public abstract class ValueObject : IEquatable<ValueObject>
{
    public abstract IEnumerable<object> GetAtomicValues();
}

// Error — record imutável
public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
}

// Result / Result<T>
Result.Success()            // Result
Result.Success<T>(value)    // Result<T>
Result.Failure(error)       // Result
Result.Failure<T>(error)    // Result<T>
```

### 3.2 Padrão de Entidade OFX

```csharp
public sealed class FinancialInstitution : AggregateRoot
{
    public string BankId { get; private set; } = null!;
    public string? OrgName { get; private set; }   // nullable — Bradesco não tem OrgName no OFX
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private FinancialInstitution() : base(0) { }   // EF Core — OBRIGATÓRIO

    private FinancialInstitution(string bankId, ...) : base(0) { ... }

    public static Result<FinancialInstitution> Create(string bankId, string? orgName, string? fid)
    {
        if (string.IsNullOrWhiteSpace(bankId))
            return Result.Failure<FinancialInstitution>(
                new Error("FinancialInstitution.EmptyBankId", "Bank ID is required."));
        return Result.Success(new FinancialInstitution(...));
    }

    public Result UpdateDetails(string? orgName, string? fid) { ... }
    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }
}
```

**Regras fixas do Domain:**
- Classe `sealed`.
- Propriedades com `private set` — **nunca** `public set`.
- Strings não anuláveis com `= null!` (resolve CS8618 do EF Core).
- Strings anuláveis com `string?` sem inicializador.
- Construtor `private` vazio com `base(0)` para EF Core.
- Construtor `private` com parâmetros para a factory.
- Factory `static Result<T> Create(...)`.
- `UpdateDetails(...)` retorna `Result`.
- `Deactivate()` atualiza `IsActive = false` + `UpdatedAt`.
- `Id` é `long` (BIGINT), não `int`.

### 3.3 Especificidades OFX no Domain

```csharp
// AcctType válidos
["CHECKING", "SAVINGS", "MONEYMRKT", "CREDITLINE", "CD", "OTHER"]

// TrnType válidos
["CREDIT","DEBIT","INT","DIV","FEE","SRVCHG","DEP","ATM","POS",
 "XFER","CHECK","PAYMENT","CASH","DIRECTDEP","DIRECTDEBIT","REPEATPMT","OTHER"]

// BalanceType válidos
["LEDGER", "AVAILABLE"]

// FileHash: SHA-256 hex — sempre 64 caracteres
// FitId: pode ser NULL (BB) ou repetido entre statements distintos
// DtPosted: NULL aceito para transações com data inválida (ano < 1753)
// OrgName: NULL aceito — Bradesco não inclui no OFX
// BranchId: NULL aceito — Bradesco não inclui BRANCHID
```

### 3.4 Interfaces de Repositório (Domain/Repositories/)

```csharp
IFinancialInstitutionRepository
  GetByIdAsync(long id, CancellationToken ct)
  GetAllAsync(CancellationToken ct)
  ExistsAsync(string bankId, string? fid, CancellationToken ct)
  AddAsync(FinancialInstitution entity, CancellationToken ct)

IBankAccountRepository
  GetByIdAsync / GetAllAsync / GetByInstitutionAsync
  ExistsAsync(string bankId, string? branchId, string acctId, CancellationToken ct)
  AddAsync

IOfxImportRepository
  GetByIdAsync / GetAllAsync
  ExistsByHashAsync(string fileHash, CancellationToken ct)   -- deduplicação SHA-256
  AddAsync

IOfxStatementRepository
  GetByIdAsync / GetAllAsync / GetByImportAsync / GetByBankAccountAsync
  AddAsync

IOfxTransactionRepository
  GetByIdAsync / GetByStatementAsync
  GetByBankAccountAsync(long bankAccountId, DateTime? from, DateTime? to, CancellationToken ct)
  AddRangeAsync(IEnumerable<OfxTransaction> transactions, CancellationToken ct)

IUnitOfWork
  CommitAsync(CancellationToken ct) → Task<int>
```

---

## 4. Application Layer — CQRS

### 4.1 Interfaces de Messaging (Application/Abstractions/Messaging/)

```csharp
public interface ICommand : IRequest<Result> { }
public interface ICommand<TResponse> : IRequest<Result<TResponse>> { }
public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand { }
public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse> { }
public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }
public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse> { }
```

> **SEMPRE** adicionar `using OpenFinancialExchange.Application.Abstractions.Messaging;` em todo Command/Query.

### 4.2 Estrutura de Features (CRUD padrão)

```
Application/Features/{Aggregate}/
  Create/
    CreateXxxCommand.cs            ← sealed record : ICommand<long>
    CreateXxxCommandHandler.cs     ← internal sealed class : ICommandHandler<Cmd, long>
    CreateXxxCommandValidator.cs   ← sealed class : AbstractValidator<Cmd>
  Update/
    UpdateXxxCommand.cs            ← sealed record : ICommand
    UpdateXxxCommandHandler.cs     ← internal sealed class : ICommandHandler<Cmd>
    UpdateXxxCommandValidator.cs
  GetAll/
    GetAllXxxQuery.cs              ← sealed record : IQuery<IReadOnlyCollection<XxxResponse>>
    GetAllXxxQueryHandler.cs       ← internal sealed class : IQueryHandler<Query, IReadOnlyCollection<XxxResponse>>
  GetById/
    GetXxxByIdQuery.cs             ← sealed record : IQuery<XxxResponse>
    GetXxxByIdQueryHandler.cs
  XxxResponse.cs                   ← sealed record (DTO de leitura)
```

**Handlers são sempre `internal sealed class`** — nunca expostos fora da Application.

### 4.3 Features Implementadas

| Feature | Commands | Queries |
|---|---|---|
| FinancialInstitutions | Create, Update | GetAll, GetById |
| BankAccounts | Create, Update | GetAll, GetById |
| OfxImports | Create | GetAll, GetById |
| OfxStatements | — | GetAll, GetById |
| OfxTransactions | — | GetByStatement, GetByBankAccount |

### 4.4 Padrão de Handler

```csharp
internal sealed class CreateOfxImportCommandHandler(
    IOfxImportRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateOfxImportCommand, long>
{
    public async Task<Result<long>> Handle(CreateOfxImportCommand request, CancellationToken cancellationToken)
    {
        // 1. Checar duplicata
        var exists = await repository.ExistsByHashAsync(request.FileHash, cancellationToken);
        if (exists)
            return Result.Failure<long>(new Error("OfxImport.Duplicate", "..."));

        // 2. Criar via factory do Domain
        var result = OfxImport.Create(...);
        if (result.IsFailure)
            return Result.Failure<long>(result.Error);

        // 3. Persistir e commitar
        await repository.AddAsync(result.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}
```

### 4.5 FluentValidation

```csharp
public sealed class CreateFinancialInstitutionCommandValidator
    : AbstractValidator<CreateFinancialInstitutionCommand>
{
    public CreateFinancialInstitutionCommandValidator()
    {
        RuleFor(x => x.BankId)
            .NotEmpty().MaximumLength(20);
        RuleFor(x => x.FileHash)
            .NotEmpty().Length(64).WithMessage("FileHash deve ser SHA-256 de 64 chars.");
        RuleFor(x => x.AcctType)
            .Must(t => ValidTypes.Contains(t?.ToUpperInvariant()))
            .WithMessage("AcctType inválido.");
    }
}
```

Regra: validações de formato/tamanho → FluentValidation. Regras de negócio → entity factory ou handler.

---

## 5. Infrastructure Layer

### 5.1 AppDbContext

```csharp
public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<FinancialInstitution> FinancialInstitutions => Set<FinancialInstitution>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<OfxImport> OfxImports => Set<OfxImport>();
    public DbSet<OfxStatement> OfxStatements => Set<OfxStatement>();
    public DbSet<OfxTransaction> OfxTransactions => Set<OfxTransaction>();
    public DbSet<OfxBalance> OfxBalances => Set<OfxBalance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

    public async Task<int> CommitAsync(CancellationToken ct = default)
        => await SaveChangesAsync(ct);
}
```

`AppDbContext` implementa `IUnitOfWork` diretamente — registrado como `Scoped`.

### 5.2 EF Core — Regra de Tracking

```
❌ AsNoTracking() → NUNCA em métodos chamados para UPDATE (entidade precisa ser tracked)
✅ AsNoTracking() → SEMPRE em consultas puras (GetAll, GetById leitura, GetBy*)
```

### 5.3 EF Core — Padrão de Configuração

```csharp
internal sealed class OfxTransactionConfiguration : IEntityTypeConfiguration<OfxTransaction>
{
    public void Configure(EntityTypeBuilder<OfxTransaction> builder)
    {
        builder.ToTable("OfxTransactions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.Property(x => x.TrnAmt).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.DtPosted).HasColumnType("datetime2").IsRequired();
        builder.Property(x => x.FitId).HasMaxLength(255);

        // Índice filtrado — FitId único POR statement apenas quando NOT NULL
        builder.HasIndex(x => new { x.StatementId, x.FitId })
            .IsUnique()
            .HasFilter("[FitId] IS NOT NULL")
            .HasDatabaseName("UIX_OfxTransactions_StatementId_FitId");

        // FK entre aggregates → Cascade para transações filhas do statement
        builder.HasOne<OfxStatement>()
            .WithMany()
            .HasForeignKey(x => x.StatementId)
            .HasConstraintName("FK_OfxTransactions_OfxStatements")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

**Regras de configuração:**
- `sealed`, implementa `IEntityTypeConfiguration<TEntity>`.
- Reside em `Infrastructure.Persistence.Configurations`.
- Nome termina com "Configuration".
- `OnDelete(Cascade)` apenas para entidades filhas (Transaction → Statement → Import).
- `OnDelete(Restrict)` entre aggregates (BankAccount → FinancialInstitution).
- Nunca usar `DateTime` — sempre `datetime2` no `HasColumnType`.

### 5.4 Padrão de Repositório Concreto

```csharp
internal sealed class OfxTransactionRepository(AppDbContext context) : IOfxTransactionRepository
{
    // Leitura — AsNoTracking obrigatório
    public async Task<OfxTransaction?> GetByIdAsync(long id, CancellationToken ct = default)
        => await context.OfxTransactions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    // Query cross-aggregate com join EF
    public async Task<IReadOnlyCollection<OfxTransaction>> GetByBankAccountAsync(
        long bankAccountId, DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var query = context.OfxTransactions.AsNoTracking()
            .Join(context.OfxStatements, t => t.StatementId, s => s.Id, (t, s) => new { t, s })
            .Where(x => x.s.BankAccountId == bankAccountId);

        if (from.HasValue) query = query.Where(x => x.t.DtPosted >= from.Value);
        if (to.HasValue) query = query.Where(x => x.t.DtPosted <= to.Value);

        return await query.Select(x => x.t).ToListAsync(ct);
    }

    // Inserção em lote
    public async Task AddRangeAsync(IEnumerable<OfxTransaction> transactions, CancellationToken ct = default)
        => await context.OfxTransactions.AddRangeAsync(transactions, ct);
}
```

- Classe `sealed`, termina com "Repository".
- Reside em `Infrastructure.Persistence.Repositories`.
- Ordenação **sempre em C#** após a query — nunca `ORDER BY` em SqlQuery (EF Core 9 envolve em subquery e SQL Server rejeita).

### 5.5 DependencyInjection.cs

```csharp
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services, IConfiguration configuration)
{
    services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
            sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

    services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

    services.AddScoped<IFinancialInstitutionRepository, FinancialInstitutionRepository>();
    services.AddScoped<IBankAccountRepository, BankAccountRepository>();
    services.AddScoped<IOfxImportRepository, OfxImportRepository>();
    services.AddScoped<IOfxStatementRepository, OfxStatementRepository>();
    services.AddScoped<IOfxTransactionRepository, OfxTransactionRepository>();

    return services;
}
```

---

## 6. API Layer

### 6.1 ApiController Base

```csharp
[ApiController]
[Route("api/[controller]")]
public abstract class ApiController(IMediator mediator) : ControllerBase
{
    protected readonly IMediator Mediator = mediator;

    protected IActionResult HandleFailure(Result result)
        => result.Error.Code switch
        {
            var c when c.EndsWith(".NotFound")     => NotFound(CreateProblemDetails(result)),
            var c when c.EndsWith(".AlreadyExists") => Conflict(CreateProblemDetails(result)),
            var c when c.EndsWith(".Duplicate")    => Conflict(CreateProblemDetails(result)),
            _                                      => BadRequest(CreateProblemDetails(result))
        };

    private static ProblemDetails CreateProblemDetails(Result result)
        => new() { Title = result.Error.Code, Detail = result.Error.Message };
}
```

### 6.2 Padrão de Controller

```csharp
[Authorize]
public sealed class FinancialInstitutionsController(IMediator mediator) : ApiController(mediator)
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetAllFinancialInstitutionsQuery(), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetFinancialInstitutionByIdQuery(id), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFinancialInstitutionCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsFailure
            ? HandleFailure(result)
            : CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateFinancialInstitutionRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateFinancialInstitutionCommand(id, request.OrgName, request.Fid), ct);
        return result.IsFailure ? HandleFailure(result) : NoContent();
    }
}

// Request separado do Command quando há parâmetro de rota
public sealed record UpdateFinancialInstitutionRequest(string? OrgName, string? Fid);
```

**Regras:**
- `sealed`, herda `ApiController`, tem `[Authorize]`.
- Termina com "Controller" e reside em `OpenFinancialExchange.API.Controllers`.
- Parâmetro de rota `{id:long}` (não `{id:int}` — IDs são BIGINT).
- `CreatedAtAction(nameof(GetById), ...)` para retorno 201.
- Request bodies de update são records separados (`XxxRequest`) quando há parâmetro de rota.

### 6.3 Program.cs — Configurações

```csharp
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddFluentValidationAutoValidation();

// JWT com validação de Issuer, Audience, Lifetime e assinatura
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { ... });
```

### 6.4 appsettings.json — Seções Obrigatórias

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=OpenFinancialExchange;..."
  },
  "Jwt": {
    "Secret": "...",
    "Issuer": "OpenFinancialExchange",
    "Audience": "OpenFinancialExchange.Client",
    "ExpiresInMinutes": 60
  }
}
```

---

## 7. Camadas de Teste

### 7.1 OpenFinancialExchange.Tests — Testes Unitários

xUnit + FluentAssertions + NSubstitute. Cobre Domain (entidades, Result) e Application (handlers).

```csharp
// NSubstitute para repositórios
var repository = Substitute.For<IFinancialInstitutionRepository>();
var unitOfWork = Substitute.For<IUnitOfWork>();

repository.ExistsAsync("237", "237", Arg.Any<CancellationToken>()).Returns(false);

// FluentAssertions
result.IsSuccess.Should().BeTrue();
result.Value.Should().Be(expectedId);
await repository.Received(1).AddAsync(Arg.Any<FinancialInstitution>(), Arg.Any<CancellationToken>());
```

### 7.2 OpenFinancialExchange.Specs — BDD / Gherkin

Reqnroll + Moq. Feature files em `Features/`, step definitions em `StepDefinitions/`.

```csharp
[Binding]
public sealed class FinancialInstitutionSteps
{
    private readonly Mock<IFinancialInstitutionRepository> _repository = new();
    // ...
    [Given(@"a financial institution with BankId ""(.*)"" already exists")]
    public void GivenExists(string bankId) { ... }
    [Then(@"the error code should be ""(.*)""")]
    public void ThenErrorCode(string code) => _result.Error.Code.Should().Be(code);
}
```

**Regras críticas de Reqnroll:**
- Parênteses em step patterns devem ser escapados: `\(texto\)`.
- Um step pattern só pode ter um binding em todo o projeto.

### 7.3 OpenFinancialExchange.ArchTests — Testes de Arquitetura (31 testes)

NetArchTest.Rules 1.3.2. Fronteiras verificadas:

```
Domain → não depende de Application / Infrastructure / API / MediatR / EF Core / FluentValidation
Application → não depende de Infrastructure / API / EF Core
Infrastructure → não depende de API

Handlers         → internal sealed (CommandHandler, QueryHandler)
Repositories     → internal sealed (sufixo "Repository")
EF Configurations → internal sealed (sufixo "Configuration")
Response DTOs    → sealed (sufixo "Response")
Commands         → residem em Application.Features
Queries          → residem em Application.Features
Validators       → residem em Application.Features
Validators       → herdam AbstractValidator<>
FinancialInstitution, BankAccount, OfxImport, OfxStatement → herdam AggregateRoot
OfxTransaction, OfxBalance → herdam Entity
Repositórios de domínio → interfaces em Domain.Repositories
Repositórios concretos  → em Infrastructure.Persistence.Repositories
EF Configurations       → em Infrastructure.Persistence.Configurations
IUnitOfWork             → em Domain.Repositories
```

---

## 8. OFX — Regras de Parsing e Importação

### 8.1 Formato OFX (SGML)

```
OFXHEADER:100
DATA:OFXSGML
VERSION:151
ENCODING:USASCII
CHARSET:1252
...

<OFX>
  <SIGNONMSGSRSV1>
    <SONRS>
      <STATUS><CODE>0<SEVERITY>INFO</STATUS>
      <DTSERVER>20260213120000[-3:BRT]
    </SONRS>
  </SIGNONMSGSRSV1>
  <BANKMSGSRSV1>
    <STMTTRNRS>
      <STMTRS>
        <CURDEF>BRL
        <BANKACCTFROM>
          <BANKID>237
          <BRANCHID>1234     ← Bradesco não tem este campo
          <ACCTID>000123456-7
          <ACCTTYPE>CHECKING
        </BANKACCTFROM>
        <BANKTRANLIST>
          <STMTTRN>
            <TRNTYPE>DEBIT
            <DTPOSTED>20260201120000[-3:BRT]
            <TRNAMT>-150.00
            <FITID>20260201001
            <NAME>Transferência
            <MEMO>TED para João
          </STMTTRN>
        </BANKTRANLIST>
        <LEDGERBAL>
          <BALAMT>1500.00
          <DTASOF>20260213000000[-3:BRT]
        </LEDGERBAL>
      </STMTRS>
    </STMTTRNRS>
  </BANKMSGSRSV1>
</OFX>
```

### 8.2 Diferenças Bradesco × Banco do Brasil

| Campo | Bradesco | Banco do Brasil |
|---|---|---|
| `OrgName` | **NULL** (não inclui `<FI><ORG>`) | "BANCO DO BRASIL" |
| `BranchId` | **NULL** (não inclui `<BRANCHID>`) | inclui agência |
| `FitId` | Preenchido e único por statement | Pode ser vazio/NULL |
| Datas inválidas | Raro | `00021130000000` (saldo sentinela) |

### 8.3 Regras de Data OFX

- Formato: `YYYYMMDDHHmmss[offset:TZ]` → converter para `DATETIME2`.
- Datas com ano < 1753 → gravar como **NULL** (limite inferior do SQL Server).
- Nunca rejeitar transação por data inválida — inserir com `DtPosted = NULL`.

### 8.4 Deduplicação de Arquivos

- Cada arquivo OFX tem um hash SHA-256 de 64 chars.
- `OfxImport.ExistsByHashAsync(hash)` impede reimportação do mesmo arquivo.
- FitId vazio ou duplicado dentro do mesmo statement → tratar como NULL (índice filtrado ignora NULLs).

---

## 9. Checklist — Nova Feature CRUD

- [ ] **Domain:** entidade `sealed`; `private set`; `= null!` para strings não anuláveis; construtor `private` vazio `base(0)`; factory `static Result<T> Create(...)` com validações de negócio; `UpdateDetails(...)` retorna `Result`; `Deactivate()`; interface `IXxxRepository` em `Domain/Repositories/`
- [ ] **EF Config:** `internal sealed`, `IEntityTypeConfiguration<T>`; `datetime2` para datas; `decimal(18,2)` para valores; índices com `HasDatabaseName`; `OnDelete(Cascade)` para filhos, `Restrict` entre aggregates; `DbSet<T>` no `AppDbContext`
- [ ] **Repositório:** `internal sealed`, sufixo "Repository", em `Infrastructure/Persistence/Repositories/`; `AsNoTracking()` em todas as leituras; sem `AsNoTracking()` em métodos de update
- [ ] **DI:** `services.AddScoped<IXxxRepository, XxxRepository>()` em `DependencyInjection.cs`
- [ ] **Commands:** `using OpenFinancialExchange.Application.Abstractions.Messaging;`; handler `internal sealed`; `await unitOfWork.CommitAsync(ct)`; validator `sealed : AbstractValidator<>`
- [ ] **Queries:** `AsNoTracking()`; `XxxResponse.cs` como `sealed record`; ordenação em C# (não no SQL)
- [ ] **Controller:** `sealed`, herda `ApiController`, `[Authorize]`; parâmetro de rota `{id:long}`; `CreatedAtAction(nameof(GetById), ...)`; request body separado do command quando há parâmetro de rota
- [ ] **SQL:** colunas `BIGINT IDENTITY`, `DATETIME2`, `DECIMAL(18,2)`; `IsActive`, `CreatedAt`, `UpdatedAt`; FK com nome explícito; soft delete
- [ ] **Testes:** unit test com NSubstitute para handler; BDD feature file + step definition; confirmar 31 testes de arquitetura passando

---

## 10. Restrições Absolutas

1. **Não alterar o esquema SQL** (`ofx_database_model_sqlserver.sql`) sem confirmação explícita.
2. **Não alterar features já homologadas** — apenas adicionar novas.
3. **Nunca `ORDER BY` dentro de `SqlQuery<T>`** — EF Core 9 quebra com SQL Server (envolve em subquery, SQL Server rejeita ORDER BY sem TOP/OFFSET).
4. **Nunca DELETE físico** — sempre soft delete (`IsActive = 0`).
5. **Nunca `AsNoTracking()` em repositório usado para update**.
6. **Nunca referenciar MediatR, EF Core ou FluentValidation no Domain** — viola o teste de arquitetura e a regra de zero dependências externas.
7. **Nunca `public set` em propriedades de entidade** — viola testes de arquitetura.
8. **Nunca criar entidade sem `sealed`** — viola testes de arquitetura.
9. **Nunca criar handler sem sufixo "CommandHandler" ou "QueryHandler"** — viola testes de arquitetura.
10. **Nunca verificar senha em SQL** — hash BCrypt sempre verificado em C# via `BCrypt.Net.BCrypt.Verify()`.
11. **Nunca usar `DateTime` no banco** — sempre `DATETIME2` (T-SQL) e `DateTime` no C# com `HasColumnType("datetime2")`.
12. **Nunca usar `int` para IDs** — todos os IDs são `long` (BIGINT IDENTITY).
13. **Nunca reimportar arquivo OFX sem checar `FileHash`** — `ExistsByHashAsync` obrigatório antes de `AddAsync` em `CreateOfxImportCommandHandler`.
