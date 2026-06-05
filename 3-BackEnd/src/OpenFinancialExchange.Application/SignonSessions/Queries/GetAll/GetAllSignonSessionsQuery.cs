using MediatR;
using OpenFinancialExchange.Application.SignonSessions.DTOs;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.SignonSessions.Queries.GetAll;

public sealed record GetAllSignonSessionsQuery : IRequest<Result<IReadOnlyList<SignonSessionDto>>>;
