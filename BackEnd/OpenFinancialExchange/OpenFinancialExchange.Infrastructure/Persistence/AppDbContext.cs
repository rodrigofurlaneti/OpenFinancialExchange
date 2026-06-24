using Microsoft.EntityFrameworkCore;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Primitives;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IUnitOfWork
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
