using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenFinancialExchange.Application.Abstractions;
using OpenFinancialExchange.Domain.Repositories;
using OpenFinancialExchange.Infrastructure.Persistence;
using OpenFinancialExchange.Infrastructure.Persistence.Repositories;
using OpenFinancialExchange.Infrastructure.Security;

namespace OpenFinancialExchange.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IFinancialInstitutionRepository, FinancialInstitutionRepository>();
        services.AddScoped<IBankAccountRepository, BankAccountRepository>();
        services.AddScoped<IOfxImportRepository, OfxImportRepository>();
        services.AddScoped<IOfxStatementRepository, OfxStatementRepository>();
        services.AddScoped<IOfxTransactionRepository, OfxTransactionRepository>();

        // Security
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }
}
