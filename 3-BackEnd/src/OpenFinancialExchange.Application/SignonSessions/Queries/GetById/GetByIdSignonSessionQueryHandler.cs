using MediatR;
using OpenFinancialExchange.Application.SignonSessions.DTOs;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.SignonSessions.Queries.GetById;

public sealed class GetByIdSignonSessionQueryHandler(
    ISignonSessionRepository repository)
    : IRequestHandler<GetByIdSignonSessionQuery, Result<SignonSessionDto>>
{
    public async Task<Result<SignonSessionDto>> Handle(
        GetByIdSignonSessionQuery query,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(query.Id);
        if (entity is null)
            return Result.Failure<SignonSessionDto>(Error.NotFound);

        var dto = new SignonSessionDto(
            entity.Id,
            entity.ImportId,
            entity.StatusCode,
            entity.StatusSeverity,
            entity.ServerDateRaw,
            entity.ServerDate,
            entity.Language,
            entity.CreatedAt,
            entity.UpdatedAt);

        return Result.Success(dto);
    }
}
