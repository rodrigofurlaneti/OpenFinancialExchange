using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Application.Abstractions;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) Generate(User user);
}
