using MediatR;
using OpenFinancialExchange.Application.SignonSessions.DTOs;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.SignonSessions.Queries.GetByImportId;

public sealed class GetSignonSessionByImportIdQueryHandler(
    ISignonSessionRepository repository)
    : IRequestHandler<GetSignonSessionByImportIdQuery, Result<SignonSessionDto>>
{
    public async Task<Result<SignonSessionDto>> Handle(
        GetSignonSessionByImportIdQuery query,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByImportIdAsync(query.ImportId, cancellationToken);

        if (entity is null)
            return Result.Failure<SignonSessionDto>(Error.NotFound);

        return Result.Success(new SignonSessionDto(
            entity.Id,
            entity.ImportId,
            entity.StatusCode,
            entity.StatusSeverity,
            entity.ServerDateRaw,
            entity.ServerDate,
            entity.Language,
            entity.CreatedAt,
            entity.UpdatedAt));
    }
}
