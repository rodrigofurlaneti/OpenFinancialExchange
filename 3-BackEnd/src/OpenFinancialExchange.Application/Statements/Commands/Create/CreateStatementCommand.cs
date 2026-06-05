using MediatR;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Statements.Commands.Create;

public sealed record CreateStatementCommand(
    int AccountId,
    string TRNUID,
    string StatusCode,
    string StatusSeverity,
    DateTime StartDate,
    DateTime EndDate,
    string? TimeZone) : IRequest<Result<int>>;
