using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Domain.Repositories;

public interface IBankAccountRepository
{
    Task<BankAccount?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<IReadOnlyCollection<BankAccount>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyCollection<BankAccount>> GetByInstitutionAsync(long financialInstitutionId, CancellationToken ct = default);
    Task<bool> ExistsAsync(string bankId, string? branchId, string acctId, CancellationToken ct = default);
    Task AddAsync(BankAccount bankAccount, CancellationToken ct = default);
}
