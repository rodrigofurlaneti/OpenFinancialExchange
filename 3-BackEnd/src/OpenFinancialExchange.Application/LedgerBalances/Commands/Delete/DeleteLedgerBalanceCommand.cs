using MediatR;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.LedgerBalances.Commands.Delete;

public sealed record DeleteLedgerBalanceCommand(int Id) : IRequest<Result>;
