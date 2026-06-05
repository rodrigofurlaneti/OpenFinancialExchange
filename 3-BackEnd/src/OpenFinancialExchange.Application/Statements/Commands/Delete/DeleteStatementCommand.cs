using MediatR;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Statements.Commands.Delete;

public sealed record DeleteStatementCommand(int Id) : IRequest<Result>;
