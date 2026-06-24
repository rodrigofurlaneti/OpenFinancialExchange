namespace OpenFinancialExchange.Application.Features.BankAccounts;

public sealed record BankAccountResponse(
    long Id,
    long FinancialInstitutionId,
    string BankId,
    string? BranchId,
    string AcctId,
    string AcctType,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
