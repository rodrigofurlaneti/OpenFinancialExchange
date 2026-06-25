using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Domain.Repositories;

public interface IOfxStatementRepository
{
    Task<OfxStatement?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<IReadOnlyCollection<OfxStatement>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyCollection<OfxStatement>> GetByImportAsync(long importId, CancellationToken ct = default);
    Task<IReadOnlyCollection<OfxStatement>> GetByBankAccountAsync(long bankAccountId, CancellationToken ct = default);
    Task AddAsync(OfxStatement ofxStatement, CancellationToken ct = default);

    /// <summary>
    /// Remove os extratos de uma importação (cascateia para transações e saldos).
    /// Usado no reprocessamento para regenerar a partir do OfxData salvo.
    /// </summary>
    Task RemoveByImportAsync(long importId, CancellationToken ct = default);
}
