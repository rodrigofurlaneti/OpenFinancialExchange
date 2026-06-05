using MediatR;
using OpenFinancialExchange.Application.LedgerBalances.DTOs;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.LedgerBalances.Queries.GetByStatementId;

public sealed record GetLedgerBalancesByStatementIdQuery(int StatementId)
    : IRequest<Result<IReadOnlyList<LedgerBalanceDto>>>;
