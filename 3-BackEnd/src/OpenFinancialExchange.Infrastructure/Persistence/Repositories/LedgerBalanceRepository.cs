using Microsoft.EntityFrameworkCore;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Infrastructure.Persistence.Repositories;

public sealed class LedgerBalanceRepository(AppDbContext context)
    : BaseRepository<LedgerBalance>(context), ILedgerBalanceRepository
{
    public async Task<IReadOnlyList<LedgerBalance>> GetByStatementIdAsync(int statementId, CancellationToken ct = default)
        => await Context.LedgerBalances
            .AsNoTracking()
            .Where(e => e.StatementId == statementId)
            .ToListAsync(ct);
}
