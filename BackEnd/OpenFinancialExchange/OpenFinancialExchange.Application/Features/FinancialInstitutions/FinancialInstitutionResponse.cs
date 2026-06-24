namespace OpenFinancialExchange.Application.Features.FinancialInstitutions;

public sealed record FinancialInstitutionResponse(
    long Id,
    string BankId,
    string? OrgName,
    string? Fid,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
