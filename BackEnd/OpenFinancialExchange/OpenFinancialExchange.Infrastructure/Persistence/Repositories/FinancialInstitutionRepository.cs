using Microsoft.EntityFrameworkCore;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Infrastructure.Persistence.Repositories;

internal sealed class FinancialInstitutionRepository(AppDbContext context) : IFinancialInstitutionRepository
{
    public async Task<FinancialInstitution?> GetByIdAsync(long id, CancellationToken ct = default)
        => await context.FinancialInstitutions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive, ct);

    public async Task<IReadOnlyCollection<FinancialInstitution>> GetAllAsync(CancellationToken ct = default)
        => await context.FinancialInstitutions
            .AsNoTracking()
            .Where(x => x.IsActive)
            .ToListAsync(ct);

    public async Task<bool> ExistsAsync(string bankId, string? fid, CancellationToken ct = default)
        => await context.FinancialInstitutions
            .AsNoTracking()
            .AnyAsync(x => x.BankId == bankId && x.Fid == fid, ct);

    public async Task AddAsync(FinancialInstitution financialInstitution, CancellationToken ct = default)
        => await context.FinancialInstitutions.AddAsync(financialInstitution, ct);
}
