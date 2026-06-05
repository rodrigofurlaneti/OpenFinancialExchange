using MediatR;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Transactions.Commands.Update;

public sealed record UpdateTransactionCommand(
    int Id,
    int? CategoryId,
    string? Memo,
    string? PayeeName,
    string? OperationSubtype) : IRequest<Result>;
