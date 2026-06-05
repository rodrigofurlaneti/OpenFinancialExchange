using Microsoft.EntityFrameworkCore;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Infrastructure.Persistence.Repositories;

public sealed class BankRepository(AppDbContext context)
    : BaseRepository<Bank>(context), IBankRepository
{
    public async Task<Bank?> GetByCOMPECodeAsync(string compeCode, CancellationToken ct = default)
        => await Context.Banks
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.COMPECode == compeCode, ct);

    public async Task<bool> COMPECodeExistsAsync(string compeCode, CancellationToken ct = default)
        => await Context.Banks
            .AnyAsync(e => e.COMPECode == compeCode, ct);
}
