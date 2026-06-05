namespace OpenFinancialExchange.Application.Transactions.DTOs;

public sealed record TransactionDto(
    int Id,
    int StatementId,
    int? CategoryId,
    string TransactionType,
    string PostedDateRaw,
    DateOnly PostedDate,
    string? TimeZone,
    decimal Amount,
    string FITID,
    string? CheckNumber,
    string? Memo,
    decimal AbsoluteAmount,
    string MovementNature,
    string? PayeeName,
    string? TransactionDateMemo,
    string? OperationSubtype,
    bool IsReconciled,
    DateTime? ReconciledAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
