using MediatR;
using OpenFinancialExchange.Application.Transactions.DTOs;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Transactions.Queries.GetByDateRange;

public sealed record GetTransactionsByDateRangeQuery(DateOnly From, DateOnly To)
    : IRequest<Result<IReadOnlyList<TransactionDto>>>;
