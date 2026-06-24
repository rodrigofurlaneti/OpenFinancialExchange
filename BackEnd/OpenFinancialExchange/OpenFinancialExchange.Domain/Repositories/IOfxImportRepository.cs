using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Domain.Repositories;

public interface IOfxImportRepository
{
    Task<OfxImport?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<IReadOnlyCollection<OfxImport>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsByHashAsync(string fileHash, CancellationToken ct = default);
    Task AddAsync(OfxImport ofxImport, CancellationToken ct = default);
}
