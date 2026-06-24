namespace OpenFinancialExchange.Application.Features.Auth;

public sealed record AuthResponse(string Token, DateTime ExpiresAt);
