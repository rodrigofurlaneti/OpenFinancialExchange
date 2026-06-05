using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Domain.Interfaces.Repositories;

public interface ISignonSessionRepository : IRepository<SignonSession>
{
    Task<SignonSession?> GetByImportIdAsync(int importId, CancellationToken ct = default);
}
