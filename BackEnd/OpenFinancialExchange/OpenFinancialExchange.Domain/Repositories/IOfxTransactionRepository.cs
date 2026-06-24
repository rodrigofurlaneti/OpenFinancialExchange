using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Domain.Repositories;

public interface IOfxTransactionRepository
{
    Task<OfxTransaction?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<IReadOnlyCollection<OfxTransaction>> GetByStatementAsync(long statementId, CancellationToken ct = default);
    Task<IReadOnlyCollection<OfxTransaction>> GetByBankAccountAsync(long bankAccountId, DateTime? from, DateTime? to, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<OfxTransaction> transactions, CancellationToken ct = default);
}
