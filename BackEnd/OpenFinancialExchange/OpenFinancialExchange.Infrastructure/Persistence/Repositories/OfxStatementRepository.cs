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
}
