using MediatR;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.SignonSessions.Commands.Update;

public sealed record UpdateSignonSessionCommand(
    int Id,
    string StatusCode,
    string StatusSeverity,
    string Language) : IRequest<Result>;
