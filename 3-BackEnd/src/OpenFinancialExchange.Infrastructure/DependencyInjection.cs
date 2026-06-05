using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenFinancialExchange.Domain.Interfaces;
using OpenFinancialExchange.Domain.Interfaces.Repositories;
using OpenFinancialExchange.Infrastructure.Persistence;
using OpenFinancialExchange.Infrastructure.Persistence.Repositories;

namespace OpenFinancialExchange.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddScoped<ITransactionCategoryRepository, TransactionCategoryRepository>();
        services.AddScoped<IBankRepository,                BankRepository>();
        services.AddScoped<IImportRepository,              ImportRepository>();
        services.AddScoped<ISignonSessionRepository,       SignonSessionRepository>();
        services.AddScoped<IAccountRepository,             AccountRepository>();
        services.AddScoped<IStatementRepository,           StatementRepository>();
        services.AddScoped<ILedgerBalanceRepository,       LedgerBalanceRepository>();
        services.AddScoped<ITransactionRepository,         TransactionRepository>();

        return services;
    }
}
