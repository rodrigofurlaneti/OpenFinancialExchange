using MediatR;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Statements.Commands.Update;

public sealed record UpdateStatementCommand(
    int Id,
    string StatusCode,
    string StatusSeverity,
    DateTime StartDate,
    DateTime EndDate,
    string? TimeZone) : IRequest<Result>;
