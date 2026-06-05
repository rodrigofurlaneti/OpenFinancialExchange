using Microsoft.EntityFrameworkCore;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Infrastructure.Persistence.Repositories;

public sealed class TransactionCategoryRepository(AppDbContext context)
    : BaseRepository<TransactionCategory>(context), ITransactionCategoryRepository
{
    public async Task<TransactionCategory?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await Context.TransactionCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Code == code, ct);

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct = default)
        => await Context.TransactionCategories
            .AnyAsync(e => e.Code == code, ct);

    public async Task<IReadOnlyList<TransactionCategory>> GetActiveAsync(CancellationToken ct = default)
        => await Context.TransactionCategories
            .AsNoTracking()
            .Where(e => e.IsActive)
            .ToListAsync(ct);
}
