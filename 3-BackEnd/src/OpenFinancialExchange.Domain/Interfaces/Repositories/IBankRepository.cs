using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Domain.Interfaces.Repositories;

public interface IBankRepository : IRepository<Bank>
{
    Task<Bank?> GetByCOMPECodeAsync(string compeCode, CancellationToken ct = default);
    Task<bool> COMPECodeExistsAsync(string compeCode, CancellationToken ct = default);
}
