using Microsoft.EntityFrameworkCore;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Infrastructure.Persistence.Repositories;

internal sealed class OfxStatementRepository(AppDbContext context) : IOfxStatementRepository
{
    public async Task<OfxStatement?> GetByIdAsync(long id, CancellationToken ct = default)
        => await context.OfxStatements
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyCollection<OfxStatement>> GetAllAsync(CancellationToken ct = default)
        => await context.OfxStatements
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<IReadOnlyCollection<OfxStatement>> GetByImportAsync(long importId, CancellationToken ct = default)
        => await context.OfxStatements
            .AsNoTracking()
            .Where(x => x.ImportId == importId)
            .ToListAsync(ct);

    public async Task<IReadOnlyCollection<OfxStatement>> GetByBankAccountAsync(long bankAccountId, CancellationToken ct = default)
        => await context.OfxStatements
            .AsNoTracking()
            .Where(x => x.BankAccountId == bankAccountId)
            .ToListAsync(ct);

    public async Task AddAsync(OfxStatement ofxStatement, CancellationToken ct = default)
        => await context.OfxStatements.AddAsync(ofxStatement, ct);

    public async Task RemoveByImportAsync(long importId, CancellationToken ct = default)
    {
        // Tracked load so SaveChanges issues the deletes; DB FK cascade removes
        // the related OfxTransactions and OfxBalances.
        var statements = await context.OfxStatements
            .Where(x => x.ImportId == importId)
            .ToListAsync(ct);

        if (statements.Count > 0)
            context.OfxStatements.RemoveRange(statements);
    }
}
