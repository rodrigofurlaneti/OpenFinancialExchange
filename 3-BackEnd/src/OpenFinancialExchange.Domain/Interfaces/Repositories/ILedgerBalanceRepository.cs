using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Domain.Interfaces.Repositories;

public interface ILedgerBalanceRepository : IRepository<LedgerBalance>
{
    Task<IReadOnlyList<LedgerBalance>> GetByStatementIdAsync(int statementId, CancellationToken ct = default);
}
