using MediatR;
using OpenFinancialExchange.Application.Statements.DTOs;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Statements.Queries.GetAll;

public sealed record GetAllStatementsQuery : IRequest<Result<IReadOnlyList<StatementDto>>>;
