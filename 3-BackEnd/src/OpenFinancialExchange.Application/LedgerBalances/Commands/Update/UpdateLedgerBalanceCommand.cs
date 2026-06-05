using MediatR;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.LedgerBalances.Commands.Update;

public sealed record UpdateLedgerBalanceCommand(
    int Id,
    string BalanceType,
    decimal Amount,
    DateTime AsOfDate) : IRequest<Result>;
