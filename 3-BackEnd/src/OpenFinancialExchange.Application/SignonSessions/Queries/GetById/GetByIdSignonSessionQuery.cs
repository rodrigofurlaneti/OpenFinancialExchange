using MediatR;
using OpenFinancialExchange.Application.SignonSessions.DTOs;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.SignonSessions.Queries.GetById;

public sealed record GetByIdSignonSessionQuery(int Id) : IRequest<Result<SignonSessionDto>>;
