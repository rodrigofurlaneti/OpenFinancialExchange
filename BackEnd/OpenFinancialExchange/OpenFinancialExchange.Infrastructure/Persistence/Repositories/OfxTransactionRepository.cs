using Microsoft.EntityFrameworkCore;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Infrastructure.Persistence.Repositories;

internal sealed class OfxTransactionRepository(AppDbContext context) : IOfxTransactionRepository
{
    public async Task<OfxTransaction?> GetByIdAsync(long id, CancellationToken ct = default)
        => await context.OfxTransactions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyCollection<OfxTransaction>> GetByStatementAsync(long statementId, CancellationToken ct = default)
        => await context.OfxTransactions
            .AsNoTracking()
            .Where(x => x.StatementId == statementId)
            .ToListAsync(ct);

    public async Task<IReadOnlyCollection<OfxTransaction>> GetByBankAccountAsync(
        long bankAccountId, DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var query = context.OfxTransactions
            .AsNoTracking()
            .Join(context.OfxStatements,
                t => t.StatementId,
                s => s.Id,
                (t, s) => new { Transaction = t, Statement = s })
            .Where(x => x.Statement.BankAccountId == bankAccountId);

        if (from.HasValue) query = query.Where(x => x.Transaction.DtPosted >= from.Value);
        if (to.HasValue) query = query.Where(x => x.Transaction.DtPosted <= to.Value);

        var result = await query.Select(x => x.Transaction).ToListAsync(ct);
        return result;
    }

    public async Task AddRangeAsync(IEnumerable<OfxTransaction> transactions, CancellationToken ct = default)
        => await context.OfxTransactions.AddRangeAsync(transactions, ct);
}
