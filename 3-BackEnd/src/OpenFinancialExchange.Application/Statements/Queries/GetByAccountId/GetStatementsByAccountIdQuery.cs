using MediatR;
using OpenFinancialExchange.Application.Statements.DTOs;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Statements.Queries.GetByAccountId;

public sealed record GetStatementsByAccountIdQuery(int AccountId)
    : IRequest<Result<IReadOnlyList<StatementDto>>>;
