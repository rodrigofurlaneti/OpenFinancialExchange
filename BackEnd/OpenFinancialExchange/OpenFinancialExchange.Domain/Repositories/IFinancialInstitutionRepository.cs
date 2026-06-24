using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Domain.Repositories;

public interface IFinancialInstitutionRepository
{
    Task<FinancialInstitution?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<IReadOnlyCollection<FinancialInstitution>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsAsync(string bankId, string? fid, CancellationToken ct = default);
    Task AddAsync(FinancialInstitution financialInstitution, CancellationToken ct = default);
}
