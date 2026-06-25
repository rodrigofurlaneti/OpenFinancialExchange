using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Domain.Repositories;

public interface IOfxTransactionRepository
{
    Task<OfxTransaction?> GetByIdAsync(long id, CancellationToken ct = default);

    /// <summary>Versão rastreada (tracked) para alterar a categoria da transação.</summary>
    Task<OfxTransaction?> GetByIdForUpdateAsync(long id, CancellationToken ct = default);
    Task<IReadOnlyCollection<OfxTransaction>> GetByStatementAsync(long statementId, CancellationToken ct = default);
    Task<IReadOnlyCollection<OfxTransaction>> GetByBankAccountAsync(long bankAccountId, DateTime? from, DateTime? to, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<OfxTransaction> transactions, CancellationToken ct = default);

    /// <summary>
    /// Returns the subset of <paramref name="fitIds"/> that already exist
    /// for the given bank account (cross-statement deduplication).
    /// </summary>
    Task<IReadOnlySet<string>> GetExistingFitIdsAsync(
        long bankAccountId, IEnumerable<string> fitIds, CancellationToken ct = default);
    Task<FinancialSummaryData> GetSummaryAsync(DateTime from, DateTime to, CancellationToken ct = default);
}

public sealed record FinancialSummaryData(
    decimal TotalCredits,
    decimal TotalDebits,
    int TransactionCount,
    IReadOnlyList<TypeSummaryData> ByType);

public sealed record TypeSummaryData(string TrnType, decimal Total, int Count);
