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
    decimal InternalCredits,
    decimal InternalDebits,
    int TransactionCount,
    IReadOnlyList<TypeSummaryData> ByType,
    IReadOnlyList<CategorySummaryData> ByCategory);

public sealed record TypeSummaryData(string TrnType, decimal Total, int Count);

/// <summary>
/// Aggregated totals for one category within the period.
/// <paramref name="CategoryId"/> is null for uncategorized transactions.
/// Credit/Debit are kept separate so the UI can switch between expenses and income.
/// <paramref name="IsInternal"/> marks transfers/investment movements excluded from totals.
/// </summary>
public sealed record CategorySummaryData(
    long? CategoryId,
    string CategoryName,
    string? Color,
    bool IsInternal,
    decimal Credit,
    decimal Debit,
    int Count);
