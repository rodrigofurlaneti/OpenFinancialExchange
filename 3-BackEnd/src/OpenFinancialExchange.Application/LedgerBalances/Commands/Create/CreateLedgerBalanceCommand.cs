using MediatR;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.LedgerBalances.Commands.Create;

public sealed record CreateLedgerBalanceCommand(
    int StatementId,
    string BalanceType,
    decimal Amount,
    DateTime AsOfDate) : IRequest<Result<int>>;
