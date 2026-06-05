using MediatR;
using OpenFinancialExchange.Application.SignonSessions.DTOs;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.SignonSessions.Queries.GetByImportId;

public sealed record GetSignonSessionByImportIdQuery(int ImportId)
    : IRequest<Result<SignonSessionDto>>;
