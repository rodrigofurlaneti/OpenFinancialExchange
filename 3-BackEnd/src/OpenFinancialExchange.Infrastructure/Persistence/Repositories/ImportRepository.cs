using Microsoft.EntityFrameworkCore;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Infrastructure.Persistence.Repositories;

public sealed class ImportRepository(AppDbContext context)
    : BaseRepository<Import>(context), IImportRepository
{
    public async Task<IReadOnlyList<Import>> GetByFileNameAsync(string fileName, CancellationToken ct = default)
        => await Context.Imports
            .AsNoTracking()
            .Where(e => e.FileName == fileName)
            .ToListAsync(ct);
}
