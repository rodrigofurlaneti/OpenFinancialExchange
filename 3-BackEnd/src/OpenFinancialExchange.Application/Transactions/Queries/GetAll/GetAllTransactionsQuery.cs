using MediatR;
using OpenFinancialExchange.Application.Transactions.DTOs;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Transactions.Queries.GetAll;

public sealed record GetAllTransactionsQuery : IRequest<Result<IReadOnlyList<TransactionDto>>>;
