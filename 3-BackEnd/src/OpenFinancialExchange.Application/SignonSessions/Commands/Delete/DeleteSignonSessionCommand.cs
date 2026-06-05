using MediatR;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.SignonSessions.Commands.Delete;

public sealed record DeleteSignonSessionCommand(int Id) : IRequest<Result>;
