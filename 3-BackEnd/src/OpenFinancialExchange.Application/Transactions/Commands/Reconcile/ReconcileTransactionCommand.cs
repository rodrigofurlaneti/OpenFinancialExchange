using MediatR;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Transactions.Commands.Reconcile;

public sealed record ReconcileTransactionCommand(int Id) : IRequest<Result>;
