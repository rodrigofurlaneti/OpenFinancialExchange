using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Domain.Interfaces.Repositories;

public interface IImportRepository : IRepository<Import>
{
    Task<IReadOnlyList<Import>> GetByFileNameAsync(string fileName, CancellationToken ct = default);
}
