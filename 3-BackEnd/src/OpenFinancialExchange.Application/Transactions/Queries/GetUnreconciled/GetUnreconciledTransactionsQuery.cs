using MediatR;
using OpenFinancialExchange.Application.Transactions.DTOs;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Transactions.Queries.GetUnreconciled;

public sealed record GetUnreconciledTransactionsQuery
    : IRequest<Result<IReadOnlyList<TransactionDto>>>;
