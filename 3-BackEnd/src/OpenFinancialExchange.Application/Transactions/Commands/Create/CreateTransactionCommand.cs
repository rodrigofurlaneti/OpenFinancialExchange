using MediatR;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Transactions.Commands.Create;

public sealed record CreateTransactionCommand(
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
    string? PayeeName,
    string? TransactionDateMemo,
    string? OperationSubtype) : IRequest<Result<int>>;
