namespace OpenFinancialExchange.Domain.Repositories;

public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken ct = default);
}
