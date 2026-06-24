using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Domain.Repositories;

public interface IOfxStatementRepository
{
    Task<OfxStatement?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<IReadOnlyCollection<OfxStatement>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyCollection<OfxStatement>> GetByImportAsync(long importId, CancellationToken ct = default);
    Task<IReadOnlyCollection<OfxStatement>> GetByBankAccountAsync(long bankAccountId, CancellationToken ct = default);
    Task AddAsync(OfxStatement ofxStatement, CancellationToken ct = default);
}
