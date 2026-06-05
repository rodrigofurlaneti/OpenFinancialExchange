using MediatR;
using OpenFinancialExchange.Application.Transactions.DTOs;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Transactions.Queries.GetById;

public sealed record GetByIdTransactionQuery(int Id) : IRequest<Result<TransactionDto>>;
