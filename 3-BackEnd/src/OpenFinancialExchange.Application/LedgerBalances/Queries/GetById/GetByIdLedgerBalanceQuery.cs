using MediatR;
using OpenFinancialExchange.Application.LedgerBalances.DTOs;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.LedgerBalances.Queries.GetById;

public sealed record GetByIdLedgerBalanceQuery(int Id) : IRequest<Result<LedgerBalanceDto>>;
