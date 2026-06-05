using MediatR;
using OpenFinancialExchange.Application.LedgerBalances.DTOs;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.LedgerBalances.Queries.GetAll;

public sealed record GetAllLedgerBalancesQuery : IRequest<Result<IReadOnlyList<LedgerBalanceDto>>>;
