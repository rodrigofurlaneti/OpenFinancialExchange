using MediatR;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Transactions.Commands.Delete;

public sealed record DeleteTransactionCommand(int Id) : IRequest<Result>;
