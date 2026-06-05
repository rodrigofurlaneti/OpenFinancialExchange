using Microsoft.EntityFrameworkCore;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Infrastructure.Persistence.Repositories;

public sealed class StatementRepository(AppDbContext context)
    : BaseRepository<Statement>(context), IStatementRepository
{
    public async Task<IReadOnlyList<Statement>> GetByAccountIdAsync(int accountId, CancellationToken ct = default)
        => await Context.Statements
            .AsNoTracking()
            .Where(e => e.AccountId == accountId)
            .ToListAsync(ct);

    public async Task<Statement?> GetWithBalancesAsync(int id, CancellationToken ct = default)
        => await Context.Statements
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, ct);
}
