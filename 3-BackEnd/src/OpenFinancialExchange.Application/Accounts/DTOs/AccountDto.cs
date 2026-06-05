namespace OpenFinancialExchange.Application.Accounts.DTOs;

public sealed record AccountDto(
    int Id,
    int ImportId,
    int BankId,
    string? BranchNumber,
    string AccountNumber,
    string AccountType,
    string DefaultCurrency,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
