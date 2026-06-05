using MediatR;
using OpenFinancialExchange.Application.Transactions.DTOs;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Transactions.Queries.GetByCategoryId;

public sealed record GetTransactionsByCategoryIdQuery(int CategoryId)
    : IRequest<Result<IReadOnlyList<TransactionDto>>>;
