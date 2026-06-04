# Architecture & Testing Patterns — OpenFinanciallExchange

## Skill Purpose

You are an expert fullstack architect. When activated, you enforce all the architecture, modeling, and testing patterns documented here. Apply them consistently whenever creating new features, reviewing code, or scaffolding anything in this solution.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 9 / C# 13 |
| Web API | ASP.NET Core Web API |
| ORM | EF Core 9 (SQL Server) |
| Messaging | MediatR 12 |
| Validation | FluentValidation 11 |
| Auth | JWT Bearer (BCrypt + System.IdentityModel.Tokens.Jwt) |
| Logging | Serilog |
| AI Integration | Google Gemini Vision API (via typed HttpClient) |
| Testing | Reqnroll (Gherkin/BDD) + xUnit + NSubstitute + FluentAssertions |
| Frontend | React 19, TypeScript 6, Vite 8 |
| Frontend State | Zustand 5 (auth) + TanStack React Query 5 (server state) |
| Forms | React Hook Form 7 + Zod 4 |
| HTTP Client | Axios (singleton with interceptors) |
| Routing | React Router DOM 7 |

---

## 1. Solution Structure

```
/
├── 2-Sql/                          # DDL scripts and seed data
├── 4-FrontEnd/                     # React + TypeScript SPA
│   └── src/
│       ├── core/                   # Axios client, auth store, router, query client
│       ├── design-system/          # Shared UI components and CSS tokens
│       ├── features/               # Feature-sliced modules
│       └── shared/                 # Cross-cutting components (Spinner, etc.)
└── 3-BackEnd/
    ├── src/
    │   ├── OpenFinanciallExchange.Domain          # Entities, Value Objects, Interfaces, Events
    │   ├── OpenFinanciallExchange.Application     # CQRS commands/queries, validators, DTOs
    │   ├── OpenFinanciallExchange.Infrastructure  # EF Core, repositories, JWT, Gemini, Serilog
    │   └── OpenFinanciallExchange.Api             # Controllers, middleware, DI composition
    └── tests/
        └── OpenFinanciallExchange.BddTests        # BDD/Gherkin feature files + step definitions
```

**Dependency flow (Clean Architecture):**
```
Api → Application → Domain
         ↑
  Infrastructure (implements Domain interfaces)
```

- Domain has **zero** external dependencies.
- Application depends only on Domain.
- Infrastructure implements Domain interfaces; never referenced by Application.
- Api references both Application and Infrastructure for DI wiring only.

---

## 2. Domain Layer

### 2.1 Base Classes

#### `Entity` (abstract)

```csharp
public abstract class Entity : IEquatable<Entity>
{
    public Guid Id { get; private init; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    protected Entity() => Id = Guid.NewGuid();
    protected Entity(Guid id) => Id = id;

    protected void SetCreatedAt(DateTime createdAt) => CreatedAt = createdAt;
    protected void SetUpdatedAt(DateTime updatedAt) => UpdatedAt = updatedAt;

    public bool Equals(Entity? other) => other is not null && Id == other.Id;
    public override bool Equals(object? obj) => obj is Entity e && Equals(e);
    public override int GetHashCode() => Id.GetHashCode();
    public static bool operator ==(Entity? a, Entity? b) => a?.Equals(b) ?? b is null;
    public static bool operator !=(Entity? a, Entity? b) => !(a == b);
}
```

#### `AggregateRoot` (abstract, extends Entity)

```csharp
public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

#### `ValueObject` (abstract)

```csharp
public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object> GetEqualityComponents();

    public bool Equals(ValueObject? other) =>
        other is not null && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

    public override bool Equals(object? obj) => obj is ValueObject vo && Equals(vo);
    public override int GetHashCode() =>
        GetEqualityComponents().Aggregate(0, HashCode.Combine);
}
```

### 2.2 Result Pattern (Railway-Oriented Programming)

All operations that can fail return `Result` or `Result<T>`. Never throw exceptions for expected business errors inside Application handlers.

```csharp
// Success
Result.Success()              // Result (void)
Result.Success<T>(value)      // Result<T>

// Failure
Result.Failure(Error.NotFound)
Result.Failure<T>(new Error("User.DuplicateEmail", "Email already registered."))
```

```csharp
public sealed record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("General.Null", "Value cannot be null.");
    public static readonly Error NotFound = new("General.NotFound", "Resource not found.");
}
```

**Usage in handlers:**
```csharp
var user = await _userRepository.GetByIdAsync(request.Id, ct);
if (user is null)
    return Result.Failure<UserDto>(Error.NotFound);
return Result.Success(user.ToDto());
```

### 2.3 Domain Events

Domain events are `sealed record` types implementing `IDomainEvent` (which extends `INotification`):

```csharp
public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}

public sealed record UserCreatedEvent(
    Guid EventId,
    DateTime OccurredAt,
    Guid UserId,
    string Email) : IDomainEvent;
```

**Rules:**
- Raised inside aggregate methods via `RaiseDomainEvent(new XxxEvent(...))`.
- Dispatched by `AppDbContext.CommitAsync()` **before** `SaveChangesAsync()` — if a handler fails, the save does not occur.
- `Ignore(u => u.DomainEvents)` in every EF configuration.

### 2.4 Domain Exceptions

```csharp
public sealed class DomainException(string message) : Exception(message);
```

Thrown inside entity factories/methods for invariant violations in entities that **do not** use the `Result<T>` pattern. Caught globally by `ExceptionHandlingMiddleware` → HTTP 400.

### 2.5 Entities — Patterns

**Rules:**
- Private constructor, private setters on all properties.
- Creation exclusively via `static` factory method (`Create()`).
- Factory returns `Result<T>` **or** throws `DomainException` (pick one strategy per entity and be consistent).
- Child entities of aggregates use `internal static Create()` to prevent external creation.
- `AggregateRoot` owns `List<ChildEntity>` via private backing field, exposed as `IReadOnlyList`.

```csharp
public sealed class FoodItem : Entity
{
    public Guid FoodCategoryId { get; private set; }
    public string Name { get; private set; } = default!;
    public Points Points { get; private set; } = default!;

    private FoodItem() { }

    public static Result<FoodItem> Create(Guid categoryId, string name, int points)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<FoodItem>(new Error("FoodItem.InvalidName", "Name is required."));

        return Result.Success(new FoodItem
        {
            FoodCategoryId = categoryId,
            Name = name.Trim(),
            Points = new Points(points)
        });
    }

    public Result Update(string name, int points)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(new Error("FoodItem.InvalidName", "Name is required."));
        Name = name.Trim();
        Points = new Points(points);
        SetUpdatedAt(DateTime.Now);
        return Result.Success();
    }
}
```

**AggregateRoot with child collection:**
```csharp
public sealed class DailyLog : AggregateRoot
{
    private readonly List<DailyLogItem> _items = [];
    public IReadOnlyList<DailyLogItem> Items => _items.AsReadOnly();
    public Points TotalPoints { get; private set; } = Points.Zero;

    public Result AddItem(FoodItem food, decimal quantity, string? mealTime)
    {
        var item = DailyLogItem.CreateForLog(Id, food.Id, quantity, food.Points, mealTime);
        _items.Add(item);
        TotalPoints = TotalPoints.Add(item.PointsComputed);
        return Result.Success();
    }
}
```

### 2.6 Value Objects

```csharp
public sealed class Email : ValueObject
{
    public string Value { get; }

    private static readonly Regex _regex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private Email(string value) => Value = value;

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !_regex.IsMatch(email))
            return Result.Failure<Email>(new Error("Email.Invalid", "Invalid email address."));
        return Result.Success(new Email(email.Trim().ToLowerInvariant()));
    }

    protected override IEnumerable<object> GetEqualityComponents() { yield return Value; }
}

public sealed class Points : ValueObject
{
    public const int DailyLimit = 300;
    public static readonly Points Zero = new(0);

    public int Value { get; }
    public Points(int value) => Value = value >= 0 ? value : throw new DomainException("Points cannot be negative.");

    public Points Add(Points other) => new(Value + other.Value);
    public Points Subtract(Points other) => new(Math.Max(0, Value - other.Value));

    public static implicit operator int(Points p) => p.Value;

    protected override IEnumerable<object> GetEqualityComponents() { yield return Value; }
}
```

### 2.7 Repository Interfaces (defined in Domain)

```csharp
public interface IRepository<T> where T : Entity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
}

public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}
```

Specific repository interfaces extend `IRepository<T>` and add domain-specific query methods:
```csharp
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default);
}
```

---

## 3. Application Layer — CQRS

### 3.1 Structure per Feature

```
Application/
  {Feature}/
    Commands/
      {Action}/
        {Action}Command.cs            → sealed record : IRequest<Result<T>>
        {Action}CommandHandler.cs     → sealed class : IRequestHandler<>
        {Action}CommandValidator.cs   → sealed class : AbstractValidator<>
    Queries/
      {Action}/
        {Action}Query.cs
        {Action}QueryHandler.cs
    DTOs/
      {Feature}Dto.cs
```

### 3.2 Command & Query Definition

```csharp
// Command (mutates state)
public sealed record CreateUserCommand(
    string FullName,
    string Email,
    string Username,
    string Password,
    Gender Gender,
    string? PhoneNumber) : IRequest<Result<Guid>>;

// Query (read-only)
public sealed record GetUserByIdQuery(Guid UserId) : IRequest<Result<UserDto>>;
```

### 3.3 Handler Pattern

```csharp
public sealed class CreateUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (await userRepository.EmailExistsAsync(request.Email, cancellationToken))
            return Result.Failure<Guid>(new Error("User.DuplicateEmail", "Email already registered."));

        var userResult = User.Create(request.FullName, request.Email, request.Username,
            passwordHasher.Hash(request.Password), request.Gender, request.PhoneNumber);

        if (!userResult.IsSuccess)
            return Result.Failure<Guid>(userResult.Error);

        await userRepository.AddAsync(userResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(userResult.Value.Id);
    }
}
```

### 3.4 MediatR Pipeline Behaviors

Registered in this order:
1. **`LoggingBehavior<TRequest, TResponse>`** — logs request name before and after handling.
2. **`ValidationBehavior<TRequest, TResponse>`** — runs all `IValidator<TRequest>` instances; throws `FluentValidation.ValidationException` if any fail (caught by middleware → HTTP 422).

```csharp
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});
services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
```

### 3.5 FluentValidation

```csharp
public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(150);
        RuleFor(x => x.Username)
            .NotEmpty().MinimumLength(3).MaximumLength(80)
            .Matches("^[a-z0-9._]+$").WithMessage("Username may only contain lowercase letters, digits, dots, and underscores.");
        RuleFor(x => x.Password)
            .NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");
        RuleFor(x => x.Gender).IsInEnum();
        RuleFor(x => x.PhoneNumber).MaximumLength(20).When(x => x.PhoneNumber is not null);
    }
}
```

**Rules:**
- One validator class per command.
- Validators are auto-discovered from the Application assembly.
- Domain invariants (complex business rules) go in the entity factory, not the validator.
- Application-level checks (duplicates, existence) go in the handler before calling the domain.

### 3.6 DTOs

```csharp
// All DTOs are sealed records — no AutoMapper; manual projection in query handlers
public sealed record UserDto(Guid Id, string FullName, string Email, string Username, DateTime CreatedAt);

// Query handler projection
public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken ct)
{
    var user = await _userRepository.GetByIdAsync(request.UserId, ct);
    if (user is null) return Result.Failure<UserDto>(Error.NotFound);

    return Result.Success(new UserDto(user.Id, user.FullName, user.Email.Value, user.Username, user.CreatedAt));
}
```

### 3.7 Application Service Interfaces

Defined in Application layer, implemented in Infrastructure:
```csharp
public interface ICurrentUser { Guid Id { get; } string Username { get; } bool IsAuthenticated { get; } }
public interface IPasswordHasher { string Hash(string password); bool Verify(string password, string hash); }
public interface ITokenService { string GenerateToken(User user); }
```

---

## 4. Infrastructure Layer

### 4.1 AppDbContext

```csharp
public sealed class AppDbContext(DbContextOptions<AppDbContext> options, IMediator mediator)
    : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    // ... other DbSets

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch domain events BEFORE saving — atomic with persistence
        var aggregates = ChangeTracker.Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var events = aggregates.SelectMany(a => a.DomainEvents).ToList();
        aggregates.ForEach(a => a.ClearDomainEvents());

        foreach (var evt in events)
            await mediator.Publish(evt, cancellationToken);

        return await SaveChangesAsync(cancellationToken);
    }
}
```

**Registration:**
```csharp
services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
```

### 4.2 EF Core Entity Configurations

All via `IEntityTypeConfiguration<T>` — auto-discovered with `ApplyConfigurationsFromAssembly`.

```csharp
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        // Value object — OwnsOne maps to columns in same table
        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value).HasColumnName("Email").HasMaxLength(150).IsRequired();
            email.HasIndex(e => e.Value).IsUnique();
        });

        // Value object — HasConversion for simpler types
        builder.Property(u => u.PhoneNumber)
            .HasConversion(v => v!.Value, v => PhoneNumber.Create(v).Value);

        // Enum stored as string
        builder.Property(u => u.Gender).HasConversion<string>();

        // Domain events are NOT persisted
        builder.Ignore(u => u.DomainEvents);

        builder.HasIndex(u => u.Username).IsUnique();
    }
}

public sealed class DailyLogConfiguration : IEntityTypeConfiguration<DailyLog>
{
    public void Configure(EntityTypeBuilder<DailyLog> builder)
    {
        builder.HasKey(d => d.Id);

        // SMALLINT for Points
        builder.OwnsOne(d => d.TotalPoints, tp =>
            tp.Property(p => p.Value).HasColumnName("TotalPoints").HasColumnType("SMALLINT")
              .HasConversion(v => (short)v, v => (int)v));

        // Unique composite index
        builder.HasIndex(d => new { d.UserId, d.LogDate }).IsUnique();

        // Owned collection
        builder.HasMany(d => d.Items)
            .WithOne()
            .HasForeignKey(i => i.DailyLogId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(d => d.DomainEvents);
    }
}
```

**Key patterns:**
- `OwnsOne` for complex value objects (mapped to same table).
- `HasConversion` for simple value objects (one primitive property).
- `HasConversion<string>()` for enums.
- `SMALLINT` for `Points` → explicit `HasConversion(v => (short)v, v => (int)v)`.
- All aggregate roots: `builder.Ignore(u => u.DomainEvents)`.
- Unique indexes for business-key uniqueness constraints.
- `OnDelete(DeleteBehavior.Cascade)` for owned collections.

### 4.3 Repository Implementation

```csharp
public abstract class BaseRepository<T>(AppDbContext context) : IRepository<T>
    where T : Entity
{
    protected readonly AppDbContext Context = context;
    protected DbSet<T> DbSet => Context.Set<T>();

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await DbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default) =>
        await DbSet.AsNoTracking().ToListAsync(ct);

    public virtual async Task AddAsync(T entity, CancellationToken ct = default) =>
        await DbSet.AddAsync(entity, ct);

    public virtual async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        // Only attach if detached — change tracker may already be tracking it
        if (Context.Entry(entity).State == EntityState.Detached)
            DbSet.Update(entity);
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken ct = default) =>
        DbSet.Remove(entity);
}
```

**Rules:**
- Always `AsNoTracking()` on reads unless the entity will be modified in the same scope.
- Override `GetByIdAsync` when eager loading is needed (`Include().ThenInclude()`).
- When EF tracking causes issues with private backing fields (e.g., `List<T>` via property), use `ExecuteUpdateAsync` raw SQL instead.

### 4.4 Infrastructure Services

**JWT:**
```csharp
public sealed class JwtTokenService(IOptions<JwtSettings> settings) : ITokenService
{
    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        // Sign with HmacSha256, 8-hour expiration
    }
}
```

**Password hashing:**
```csharp
public sealed class PasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
```

**Current user (reads from HttpContext):**
```csharp
public sealed class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid Id => Guid.Parse(accessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    public string Username => accessor.HttpContext!.User.FindFirstValue(ClaimTypes.Name)!;
    public bool IsAuthenticated => accessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
```

**External AI integration (Anti-Corruption Layer):**
```csharp
// Pure HTTP client — single responsibility
public sealed class GeminiHttpClient(HttpClient httpClient, IOptions<GeminiSettings> settings) { ... }

// Domain translation — wraps HTTP client, returns domain types
public sealed class GeminiVisionService(GeminiHttpClient client) : IGeminiVisionService
{
    public async Task<FoodPhotoAnalysisResult> AnalyzeAsync(byte[] imageBytes, string mimeType, CancellationToken ct)
    {
        // Build multipart vision request with base64-encoded image
        // Parse JSON response, strip markdown fences
        // Return domain record FoodPhotoAnalysisResult
    }
}
```

---

## 5. API Layer

### 5.1 BaseApiController

```csharp
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController(IMediator mediator) : ControllerBase
{
    protected readonly IMediator Mediator = mediator;

    protected IActionResult FromResult<T>(Result<T> result) =>
        result.IsSuccess ? Ok(result.Value) : Problem(result.Error);

    protected IActionResult FromResult(Result result) =>
        result.IsSuccess ? NoContent() : Problem(result.Error);

    private IActionResult Problem(Error error) => error.Code switch
    {
        "General.NotFound" => NotFound(new { error.Code, error.Description }),
        _ => BadRequest(new { error.Code, error.Description })
    };
}
```

### 5.2 Controller Pattern

```csharp
[Authorize]
public sealed class DailyLogsController(IMediator mediator, ICurrentUser currentUser)
    : BaseApiController(mediator)
{
    [HttpGet("{date:datetime}")]
    public async Task<IActionResult> GetByDate(DateTime date, CancellationToken ct) =>
        FromResult(await Mediator.Send(new GetDailyLogByDateQuery(currentUser.Id, date), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDailyLogRequest request, CancellationToken ct) =>
        FromResult(await Mediator.Send(
            new CreateDailyLogCommand(currentUser.Id, request.LogDate, request.Notes), ct));
}

// Request DTOs — inline sealed records in controller file
internal sealed record CreateDailyLogRequest(DateTime LogDate, string? Notes);
```

### 5.3 Exception Handling Middleware

```csharp
// Registered FIRST in the pipeline
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (DomainException ex)
        {
            context.Response.StatusCode = 400;
            await WriteError(context, "DOMAIN_ERROR", ex.Message);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = 422;
            await WriteValidationErrors(context, ex.Errors);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = 500;
            await WriteError(context, "INTERNAL_ERROR", "An unexpected error occurred.");
        }
    }
}
```

**Error response formats:**
```json
// DomainException → 400
{ "code": "DOMAIN_ERROR", "message": "Points cannot be negative." }

// ValidationException → 422
{ "code": "VALIDATION_ERROR", "message": "Validation failed.", "errors": [{ "property": "Email", "message": "Invalid email address." }] }

// Unhandled → 500
{ "code": "INTERNAL_ERROR", "message": "An unexpected error occurred." }
```

---

## 6. Database Schema

All PKs are `UNIQUEIDENTIFIER`. All tables have `CreatedAt` and `UpdatedAt` where applicable.

```sql
-- Users
CREATE TABLE Users (
    Id              UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    FullName        NVARCHAR(150)   NOT NULL,
    Email           NVARCHAR(150)   NOT NULL UNIQUE,
    PhoneNumber     NVARCHAR(20),
    BirthDate       DATE,
    Gender          NVARCHAR(10)    NOT NULL,  -- 'Male' | 'Female' | 'Other'
    Username        NVARCHAR(80)    NOT NULL UNIQUE,
    PasswordHash    NVARCHAR(255)   NOT NULL,
    IsActive        BIT             NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2       NOT NULL,
    UpdatedAt       DATETIME2
);

-- DailyLog — one per user per date
CREATE TABLE DailyLog (
    Id              UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId          UNIQUEIDENTIFIER NOT NULL REFERENCES Users(Id),
    LogDate         DATE             NOT NULL,
    TotalPoints     SMALLINT         NOT NULL DEFAULT 0,
    Notes           NVARCHAR(500),
    CreatedAt       DATETIME2        NOT NULL,
    UpdatedAt       DATETIME2,
    CONSTRAINT UQ_DailyLog_User_Date UNIQUE (UserId, LogDate)
);

-- DailyLogItem
CREATE TABLE DailyLogItem (
    Id              UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    DailyLogId      UNIQUEIDENTIFIER NOT NULL REFERENCES DailyLog(Id) ON DELETE CASCADE,
    FoodItemId      UNIQUEIDENTIFIER NOT NULL REFERENCES FoodItem(Id),
    Quantity        DECIMAL(5,2)     NOT NULL,
    PointsComputed  SMALLINT         NOT NULL,
    MealTime        NVARCHAR(30),
    Notes           NVARCHAR(300)
);

-- ExamRequestItem — unique exam per request
CONSTRAINT UQ_ExamRequestItem_Request_Exam UNIQUE (ExamRequestId, ExamId)
```

**Key design decisions:**
- `SMALLINT` for all Points columns (max 32,767; daily limit is 300).
- `NVARCHAR` for all text (Unicode support).
- Soft deletes via `IsActive BIT` on reference tables (FoodItem, Exam, etc.).
- Cascade delete on owned item collections only.

---

## 7. Testing Strategy

### 7.1 BDD with Reqnroll (Gherkin)

Tests are written as Gherkin feature files. Each scenario tests one Application layer behavior in isolation.

**Feature file structure:**
```gherkin
Feature: User Registration

  Scenario: Successfully register a new user
    Given no user exists with email "john@example.com"
    When I register with email "john@example.com" and password "Password1"
    Then the registration should succeed

  Scenario: Reject duplicate email
    Given a user already exists with email "john@example.com"
    When I register with email "john@example.com" and password "Password1"
    Then the registration should fail with error "User.DuplicateEmail"

  Scenario: Reject weak password
    Given no user exists with email "test@example.com"
    When I register with email "test@example.com" and password "weak"
    Then the registration should fail with a validation error
```

### 7.2 Step Definition Pattern

```csharp
[Binding]
public sealed class UserRegistrationSteps
{
    // NSubstitute mocks — created fresh per scenario
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private Result<Guid> _result = default!;

    [Given("no user exists with email {string}")]
    public void GivenNoUserWithEmail(string email) =>
        _userRepository.EmailExistsAsync(email.ToLower()).Returns(false);

    [Given("a user already exists with email {string}")]
    public void GivenUserWithEmail(string email) =>
        _userRepository.EmailExistsAsync(email.ToLower()).Returns(true);

    [When("I register with email {string} and password {string}")]
    public async Task WhenRegister(string email, string password)
    {
        var handler = new CreateUserCommandHandler(_userRepository, _passwordHasher, _unitOfWork);
        _result = await handler.Handle(new CreateUserCommand("John Doe", email, "john", password, Gender.Male, null), default);
    }

    [Then("the registration should succeed")]
    public void ThenSucceeds() =>
        _result.IsSuccess.Should().BeTrue();

    [Then("the registration should fail with error {string}")]
    public void ThenFailsWith(string errorCode) =>
        _result.Error.Code.Should().Be(errorCode);
}
```

### 7.3 Test Frameworks and Roles

| Framework | Role |
|---|---|
| Reqnroll 2.4+ | BDD runner — Gherkin `.feature` files + `[Binding]` step classes |
| xUnit 2.9+ | Underlying test runner |
| NSubstitute 5+ | Mocking: `Substitute.For<T>()`, `.Returns()`, `.Received()` |
| FluentAssertions 6+ | Assertions: `.Should().BeTrue()`, `.Should().Be()`, `.Should().Contain()` |
| coverlet.collector | Code coverage reporting |

### 7.4 Test Scope and Coverage Areas

**What is tested:**
- Application command handlers (happy path + error cases).
- FluentValidation validators (invalid inputs rejected).
- Domain entity factories (via handler tests).

**What is NOT tested (pragmatic decision):**
- EF Core repositories (no in-memory or test-database setup).
- API controllers (no WebApplicationFactory).
- Infrastructure services (JWT, BCrypt, Gemini — tested via integration/E2E separately).

### 7.5 Naming Conventions

- Feature files: `{Feature}.feature` (e.g., `UserRegistration.feature`, `DailyLog.feature`).
- Step classes: `{Feature}Steps` (e.g., `UserRegistrationSteps`, `DailyLogSteps`).
- Scenario names: Natural English sentences describing the behavior.
- Step parameters: Use `{string}`, `{int}`, `{decimal}` Reqnroll data types.

### 7.6 Test Isolation

Each `[Binding]` class creates its own mock instances as field initializers — no shared state across scenarios. No test database, no WebApplicationFactory. All dependencies mocked via NSubstitute.

---

## 8. Frontend Architecture

### 8.1 Feature-Sliced Structure

```
src/
  core/
    api/client.ts          → Axios singleton + Bearer interceptor + 401 redirect
    auth/authStore.ts      → Zustand store (token, user, login, logout) — persisted
    auth/ProtectedRoute.tsx → Route guard — redirects to /login if not authenticated
    router/AppRouter.tsx   → createBrowserRouter with lazy-loaded pages
    queryClient.ts         → TanStack Query client configuration
  design-system/
    components/            → Button, Input, Card, Spinner, Nav, Footer
    tokens.css / tokens.ts → Design tokens (colors, spacing, typography)
    global.css
  features/
    auth/                  → api/, hooks/ (useLogin, useRegister), pages/, types/
    dashboard/             → api/, hooks/, pages/
    food-log/              → api/, hooks/, pages/, types/
    exams/                 → api/, hooks/, pages/, types/
    admin/                 → api/, hooks/, pages/ (FoodItems, FoodCategories, Exams, ExamCategories)
    users/                 → api/, hooks/, pages/, types/
  shared/
    components/Spinner/
```

### 8.2 Auth State (Zustand)

```typescript
// core/auth/authStore.ts
interface AuthState {
  token: string | null;
  user: AuthUser | null;
  login: (token: string, user: AuthUser) => void;
  logout: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      user: null,
      login: (token, user) => set({ token, user }),
      logout: () => set({ token: null, user: null }),
    }),
    { name: 'pte-auth', partialize: (s) => ({ token: s.token, user: s.user }) }
  )
);
```

### 8.3 HTTP Client

```typescript
// core/api/client.ts
export const apiClient = axios.create({ baseURL: import.meta.env.VITE_API_URL });

apiClient.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

apiClient.interceptors.response.use(
  (r) => r,
  (error) => {
    if (error.response?.status === 401) {
      useAuthStore.getState().logout();
      window.location.replace('/login');
    }
    return Promise.reject(error);
  }
);
```

### 8.4 Data Fetching Pattern (React Query)

```typescript
// features/food-log/hooks/useTodayLog.ts
export const useTodayLog = (date: string) =>
  useQuery({ queryKey: ['daily-log', date], queryFn: () => getDailyLog(date) });

// features/food-log/hooks/useAddLogItem.ts
export const useAddLogItem = (logId: string) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: AddLogItemRequest) => addLogItem(logId, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['daily-log'] }),
  });
};
```

### 8.5 Form Pattern (React Hook Form + Zod)

```typescript
const schema = z.object({
  email: z.string().email(),
  password: z.string().min(8),
});

const { register, handleSubmit, formState: { errors } } = useForm<LoginForm>({
  resolver: zodResolver(schema),
});
```

### 8.6 Route Protection

```typescript
// core/auth/ProtectedRoute.tsx
export const ProtectedRoute = () => {
  const { token } = useAuthStore();
  const location = useLocation();
  if (!token) return <Navigate to="/login" state={{ from: location }} replace />;
  return <Outlet />;
};
```

### 8.7 Code Splitting

All page-level components use `React.lazy()`:
```typescript
const DashboardPage = lazy(() => import('../features/dashboard/pages/DashboardPage'));
// Wrapped in <Suspense fallback={<Spinner fullPage />}> in AppRouter
```

---

## 9. Key Architectural Decisions

1. **No AutoMapper** — All DTO projections are manual `sealed record` construction inside query handlers. Keeps mappings explicit and searchable.

2. **Domain events dispatched pre-save** — `CommitAsync()` publishes events before `SaveChangesAsync()`. Event handlers and persistence are atomic within one DbContext scope.

3. **`AppDbContext` implements `IUnitOfWork`** — Registered as `services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>())`. No separate UoW wrapper needed.

4. **Mixed error strategies** — Some entities throw `DomainException`, others return `Result<T>`. New entities should prefer `Result<T>` for consistency.

5. **Raw SQL for EF tracking edge cases** — When EF's change tracker conflicts with private backing fields (e.g., updating `TotalPoints` after inserting a child), use `ExecuteUpdateAsync` with raw SQL rather than fighting the ORM.

6. **BDD-first testing** — Tests are written from the outside in (behavior → implementation). Gherkin scenarios serve as living documentation. No test databases; all tests are unit-level with NSubstitute mocks.

7. **Gemini Vision ACL** — External AI API has two layers: `GeminiHttpClient` (raw HTTP, single responsibility) and `GeminiVisionService` (domain translation). The domain interface `IGeminiVisionService` is defined in Domain, preventing leakage of HTTP concerns.

8. **Points formula** — `(portionGrams / 100) * caloriesPer100g * 0.24` computed in `AnalyzeFoodPhotoCommandHandler`.

---

## 10. Checklist — Adding a New Feature

When adding a new feature (e.g., `WeightLog`), follow this order:

- [ ] **Domain**: Define entity/aggregate extending `Entity` or `AggregateRoot`; private constructor + static factory returning `Result<T>`; value objects if needed; domain events if state changes matter; repository interface in `Domain/Interfaces/Repositories/`
- [ ] **EF Config**: Add `IEntityTypeConfiguration<T>` in Infrastructure; configure value objects, indexes, cascade behavior; add `DbSet<T>` to `AppDbContext`; run `dotnet ef migrations add`
- [ ] **Repository**: Implement `BaseRepository<T>` in Infrastructure; override methods that need eager loading or tracking
- [ ] **DI**: Register repository as scoped in `AddInfrastructure()`
- [ ] **Application Commands**: Create `{Action}Command` (sealed record), `{Action}CommandHandler`, `{Action}CommandValidator`
- [ ] **Application Queries**: Create `{Action}Query`, `{Action}QueryHandler`, `{Feature}Dto`
- [ ] **API Controller**: Extend `BaseApiController`; use `FromResult()` for all responses; add request DTOs as `internal sealed record` in controller file; apply `[Authorize]` if needed
- [ ] **Tests**: Write `.feature` file with scenarios for success + error cases; implement `[Binding]` step class with NSubstitute mocks and FluentAssertions
- [ ] **Frontend**: Add `types/`, `api/`, `hooks/` (useQuery + useMutation), `pages/` under `features/{feature}/`; add route to `AppRouter.tsx` with `lazy()`; add nav link if needed