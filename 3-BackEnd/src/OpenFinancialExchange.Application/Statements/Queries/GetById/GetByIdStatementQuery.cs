using MediatR;
using OpenFinancialExchange.Application.Statements.DTOs;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Statements.Queries.GetById;

public sealed record GetByIdStatementQuery(int Id) : IRequest<Result<StatementDto>>;
