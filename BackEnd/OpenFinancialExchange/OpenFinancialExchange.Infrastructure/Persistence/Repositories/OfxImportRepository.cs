using Microsoft.EntityFrameworkCore;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Infrastructure.Persistence.Repositories;

internal sealed class OfxImportRepository(AppDbContext context) : IOfxImportRepository
{
    public async Task<OfxImport?> GetByIdAsync(long id, CancellationToken ct = default)
        => await context.OfxImports
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyCollection<OfxImport>> GetAllAsync(CancellationToken ct = default)
        => await context.OfxImports
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<bool> ExistsByHashAsync(string fileHash, CancellationToken ct = default)
        => await context.OfxImports
            .AsNoTracking()
            .AnyAsync(x => x.FileHash == fileHash, ct);

    public async Task AddAsync(OfxImport ofxImport, CancellationToken ct = default)
        => await context.OfxImports.AddAsync(ofxImport, ct);
}
