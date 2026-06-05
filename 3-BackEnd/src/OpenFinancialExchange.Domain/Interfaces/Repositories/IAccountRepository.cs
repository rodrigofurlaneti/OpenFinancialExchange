using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Domain.Interfaces.Repositories;

public interface IAccountRepository : IRepository<Account>
{
    Task<IReadOnlyList<Account>> GetByImportIdAsync(int importId, CancellationToken ct = default);
    Task<IReadOnlyList<Account>> GetByBankIdAsync(int bankId, CancellationToken ct = default);
}
