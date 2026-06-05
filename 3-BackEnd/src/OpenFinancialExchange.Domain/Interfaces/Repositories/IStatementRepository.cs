using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Domain.Interfaces.Repositories;

public interface IStatementRepository : IRepository<Statement>
{
    Task<IReadOnlyList<Statement>> GetByAccountIdAsync(int accountId, CancellationToken ct = default);
    Task<Statement?> GetWithBalancesAsync(int id, CancellationToken ct = default);
}
