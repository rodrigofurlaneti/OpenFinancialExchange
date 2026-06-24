namespace OpenFinancialExchange.Application.Features.OfxTransactions;

public sealed record OfxTransactionResponse(
    long Id,
    long StatementId,
    string TrnType,
    DateTime DtPosted,
    decimal TrnAmt,
    string? FitId,
    string? Name,
    string? Memo,
    string? CheckNum,
    DateTime CreatedAt);
