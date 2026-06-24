using Microsoft.EntityFrameworkCore;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Infrastructure.Persistence.Repositories;

internal sealed class BankAccountRepository(AppDbContext context) : IBankAccountRepository
{
    public async Task<BankAccount?> GetByIdAsync(long id, CancellationToken ct = default)
        => await context.BankAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive, ct);

    public async Task<IReadOnlyCollection<BankAccount>> GetAllAsync(CancellationToken ct = default)
        => await context.BankAccounts
            .AsNoTracking()
            .Where(x => x.IsActive)
            .ToListAsync(ct);

    public async Task<IReadOnlyCollection<BankAccount>> GetByInstitutionAsync(long financialInstitutionId, CancellationToken ct = default)
        => await context.BankAccounts
            .AsNoTracking()
            .Where(x => x.FinancialInstitutionId == financialInstitutionId && x.IsActive)
            .ToListAsync(ct);

    public async Task<bool> ExistsAsync(string bankId, string? branchId, string acctId, CancellationToken ct = default)
        => await context.BankAccounts
            .AsNoTracking()
            .AnyAsync(x => x.BankId == bankId && x.BranchId == branchId && x.AcctId == acctId, ct);

    public async Task<BankAccount?> FindByAcctIdAsync(string acctId, string? bankId = null, CancellationToken ct = default)
    {
        // Fetch in memory after filtering by AcctId in the DB
        // (BankId TrimStart can't be translated to SQL)
        var accounts = await context.BankAccounts
            .AsNoTracking()
            .Where(x => x.AcctId == acctId && x.IsActive)
            .ToListAsync(ct);

        if (string.IsNullOrWhiteSpace(bankId))
            return accounts.FirstOrDefault();

        var normalizedBankId = bankId.TrimStart('0');
        return accounts.FirstOrDefault(x =>
            x.BankId == bankId || x.BankId.TrimStart('0') == normalizedBankId);
    }

    public async Task AddAsync(BankAccount bankAccount, CancellationToken ct = default)
        => await context.BankAccounts.AddAsync(bankAccount, ct);
}
