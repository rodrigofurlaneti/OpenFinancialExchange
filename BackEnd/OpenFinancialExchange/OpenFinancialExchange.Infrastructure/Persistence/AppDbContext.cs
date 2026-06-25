using Microsoft.EntityFrameworkCore;
using OpenFinancialExchange.Application.Abstractions;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUser)
    : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<BankCode> BankCodes => Set<BankCode>();
    public DbSet<FinancialInstitution> FinancialInstitutions => Set<FinancialInstitution>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<OfxImport> OfxImports => Set<OfxImport>();
    public DbSet<OfxStatement> OfxStatements => Set<OfxStatement>();
    public DbSet<OfxTransaction> OfxTransactions => Set<OfxTransaction>();
    public DbSet<OfxBalance> OfxBalances => Set<OfxBalance>();
    public DbSet<Category> Categories => Set<Category>();

    /// <summary>
    /// Tenant of the active request. EF re-evaluates this per query, so the global
    /// query filters below scope every read to the authenticated user automatically.
    /// </summary>
    private long? CurrentUserId => currentUser.UserId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // ── Multi-tenant global query filters ──────────────────────────────
        // BankCode and User are global/reference data and are intentionally excluded.
        modelBuilder.Entity<FinancialInstitution>().HasQueryFilter(e => e.UserId == CurrentUserId);
        modelBuilder.Entity<BankAccount>().HasQueryFilter(e => e.UserId == CurrentUserId);
        modelBuilder.Entity<OfxImport>().HasQueryFilter(e => e.UserId == CurrentUserId);
        modelBuilder.Entity<OfxStatement>().HasQueryFilter(e => e.UserId == CurrentUserId);
        modelBuilder.Entity<OfxTransaction>().HasQueryFilter(e => e.UserId == CurrentUserId);
        modelBuilder.Entity<OfxBalance>().HasQueryFilter(e => e.UserId == CurrentUserId);

        // System categories (UserId == null) are shared with every user.
        modelBuilder.Entity<Category>().HasQueryFilter(e => e.UserId == null || e.UserId == CurrentUserId);
    }

    public async Task<int> CommitAsync(CancellationToken ct = default)
        => await SaveChangesAsync(ct);
}
