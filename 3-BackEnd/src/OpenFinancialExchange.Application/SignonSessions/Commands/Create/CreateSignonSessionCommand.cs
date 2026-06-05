using MediatR;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.SignonSessions.Commands.Create;

public sealed record CreateSignonSessionCommand(
    int ImportId,
    string StatusCode,
    string StatusSeverity,
    string ServerDateRaw,
    DateTime? ServerDate,
    string Language) : IRequest<Result<int>>;
