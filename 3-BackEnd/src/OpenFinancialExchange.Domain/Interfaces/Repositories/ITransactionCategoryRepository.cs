using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Domain.Interfaces.Repositories;

public interface ITransactionCategoryRepository : IRepository<TransactionCategory>
{
    Task<TransactionCategory?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<TransactionCategory>> GetActiveAsync(CancellationToken ct = default);
}
