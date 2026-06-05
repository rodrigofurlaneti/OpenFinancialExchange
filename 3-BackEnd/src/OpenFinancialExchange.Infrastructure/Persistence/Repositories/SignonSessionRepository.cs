using Microsoft.EntityFrameworkCore;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Infrastructure.Persistence.Repositories;

public sealed class SignonSessionRepository(AppDbContext context)
    : BaseRepository<SignonSession>(context), ISignonSessionRepository
{
    public async Task<SignonSession?> GetByImportIdAsync(int importId, CancellationToken ct = default)
        => await Context.SignonSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ImportId == importId, ct);
}
