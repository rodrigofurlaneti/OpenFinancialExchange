using System.Reflection;
using Microsoft.EntityFrameworkCore;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Interfaces;

namespace OpenFinancialExchange.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<TransactionCategory> TransactionCategories => Set<TransactionCategory>();
    public DbSet<Bank>                Banks                 => Set<Bank>();
    public DbSet<Import>              Imports               => Set<Import>();
    public DbSet<SignonSession>       SignonSessions         => Set<SignonSession>();
    public DbSet<Account>             Accounts              => Set<Account>();
    public DbSet<Statement>           Statements            => Set<Statement>();
    public DbSet<LedgerBalance>       LedgerBalances        => Set<LedgerBalance>();
    public DbSet<Transaction>         Transactions          => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    public Task<int> CommitAsync(CancellationToken cancellationToken = default)
        => SaveChangesAsync(cancellationToken);
}
