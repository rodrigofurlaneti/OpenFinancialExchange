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

    public async Task<IReadOnlySet<string>> GetExistingFitIdsAsync(
        long bankAccountId, IEnumerable<string> fitIds, CancellationToken ct = default)
    {
        var fitIdList = fitIds.Where(f => !string.IsNullOrWhiteSpace(f)).Distinct().ToList();
        if (fitIdList.Count == 0) return new HashSet<string>();

        var existing = await context.OfxTransactions
            .AsNoTracking()
            .Join(context.OfxStatements,
                t => t.StatementId,
                s => s.Id,
                (t, s) => new { t.FitId, s.BankAccountId })
            .Where(x => x.BankAccountId == bankAccountId && x.FitId != null && fitIdList.Contains(x.FitId!))
            .Select(x => x.FitId!)
            .Distinct()
            .ToListAsync(ct);

        return new HashSet<string>(existing, StringComparer.Ordinal);
    }

    public async Task<FinancialSummaryData> GetSummaryAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var transactions = await context.OfxTransactions
            .AsNoTracking()
            .Where(t => t.DtPosted >= from && t.DtPosted <= to)
            .ToListAsync(ct);

        var totalCredits = transactions.Where(t => t.TrnAmt > 0).Sum(t => t.TrnAmt);
        var totalDebits  = transactions.Where(t => t.TrnAmt < 0).Sum(t => Math.Abs(t.TrnAmt));
        var count        = transactions.Count;

        var byType = transactions
            .GroupBy(t => t.TrnType)
            .Select(g => new TypeSummaryData(g.Key, g.Sum(t => t.TrnAmt), g.Count()))
            .OrderByDescending(x => Math.Abs(x.Total))
            .ToList();

        return new FinancialSummaryData(totalCredits, totalDebits, count, byType);
    }
}
