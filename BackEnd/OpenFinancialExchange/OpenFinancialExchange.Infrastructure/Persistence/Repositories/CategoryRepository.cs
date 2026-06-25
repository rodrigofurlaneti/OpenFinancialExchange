using Microsoft.EntityFrameworkCore;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Infrastructure.Persistence.Repositories;

internal sealed class CategoryRepository(AppDbContext context) : ICategoryRepository
{
    public async Task<Category?> GetByIdAsync(long id, CancellationToken ct = default)
        => await context.Categories
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive, ct);

    public async Task<IReadOnlyCollection<Category>> GetAllAsync(CancellationToken ct = default)
        => await context.Categories
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

    public async Task<bool> ExistsByNameAsync(string name, long? excludeId, CancellationToken ct = default)
        => await context.Categories
            .AsNoTracking()
            .AnyAsync(x => x.IsActive && x.Name == name && (excludeId == null || x.Id != excludeId), ct);

    public async Task AddAsync(Category category, CancellationToken ct = default)
        => await context.Categories.AddAsync(category, ct);
}
