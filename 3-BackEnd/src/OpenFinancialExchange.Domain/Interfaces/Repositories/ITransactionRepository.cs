using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Domain.Interfaces.Repositories;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<Transaction?> GetByFITIDAsync(string fitid, CancellationToken ct = default);
    Task<bool> FITIDExistsAsync(string fitid, CancellationToken ct = default);
    Task<IReadOnlyList<Transaction>> GetByStatementIdAsync(int statementId, CancellationToken ct = default);
    Task<IReadOnlyList<Transaction>> GetByCategoryIdAsync(int categoryId, CancellationToken ct = default);
    Task<IReadOnlyList<Transaction>> GetByDateRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<IReadOnlyList<Transaction>> GetUnreconciledAsync(CancellationToken ct = default);
}
