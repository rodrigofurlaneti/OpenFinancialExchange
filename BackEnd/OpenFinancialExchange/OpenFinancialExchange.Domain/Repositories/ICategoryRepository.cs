using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Domain.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<IReadOnlyCollection<Category>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, long? excludeId, CancellationToken ct = default);
    Task AddAsync(Category category, CancellationToken ct = default);
}
