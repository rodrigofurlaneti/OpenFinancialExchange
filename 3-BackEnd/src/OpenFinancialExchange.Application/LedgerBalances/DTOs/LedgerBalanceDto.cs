namespace OpenFinancialExchange.Application.LedgerBalances.DTOs;

public sealed record LedgerBalanceDto(
    int Id,
    int StatementId,
    string BalanceType,
    decimal Amount,
    DateTime AsOfDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
