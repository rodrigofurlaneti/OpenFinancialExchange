using MediatR;
using OpenFinancialExchange.Application.Transactions.DTOs;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Transactions.Queries.GetByStatementId;

public sealed record GetTransactionsByStatementIdQuery(int StatementId)
    : IRequest<Result<IReadOnlyList<TransactionDto>>>;
