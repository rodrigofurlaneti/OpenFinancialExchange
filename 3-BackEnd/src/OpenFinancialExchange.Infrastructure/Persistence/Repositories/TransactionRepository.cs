using Microsoft.EntityFrameworkCore;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Infrastructure.Persistence.Repositories;

public sealed class TransactionRepository(AppDbContext context)
    : BaseRepository<Transaction>(context), ITransactionRepository
{
    public async Task<Transaction?> GetByFITIDAsync(string fitid, CancellationToken ct = default)
        => await Context.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.FITID == fitid, ct);

    public async Task<bool> FITIDExistsAsync(string fitid, CancellationToken ct = default)
        => await Context.Transactions
            .AnyAsync(e => e.FITID == fitid, ct);

    public async Task<IReadOnlyList<Transaction>> GetByStatementIdAsync(int statementId, CancellationToken ct = default)
        => await Context.Transactions
            .AsNoTracking()
            .Where(e => e.StatementId == statementId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Transaction>> GetByCategoryIdAsync(int categoryId, CancellationToken ct = default)
        => await Context.Transactions
            .AsNoTracking()
            .Where(e => e.CategoryId == categoryId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Transaction>> GetByDateRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
        => await Context.Transactions
            .AsNoTracking()
            .Where(e => e.PostedDate >= from && e.PostedDate <= to)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Transaction>> GetUnreconciledAsync(CancellationToken ct = default)
        => await Context.Transactions
            .AsNoTracking()
            .Where(e => !e.IsReconciled)
            .ToListAsync(ct);
}
