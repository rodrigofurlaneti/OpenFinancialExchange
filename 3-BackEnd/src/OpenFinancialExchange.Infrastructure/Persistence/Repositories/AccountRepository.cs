using Microsoft.EntityFrameworkCore;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Infrastructure.Persistence.Repositories;

public sealed class AccountRepository(AppDbContext context)
    : BaseRepository<Account>(context), IAccountRepository
{
    public async Task<IReadOnlyList<Account>> GetByImportIdAsync(int importId, CancellationToken ct = default)
        => await Context.Accounts
            .AsNoTracking()
            .Where(e => e.ImportId == importId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Account>> GetByBankIdAsync(int bankId, CancellationToken ct = default)
        => await Context.Accounts
            .AsNoTracking()
            .Where(e => e.BankId == bankId)
            .ToListAsync(ct);
}
