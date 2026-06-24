using Microsoft.EntityFrameworkCore;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) => _context = context;

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        await _context.Users.AddAsync(user, cancellationToken);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
}
