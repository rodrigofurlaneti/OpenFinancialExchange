using MediatR;
using OpenFinancialExchange.Application.SignonSessions.DTOs;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.SignonSessions.Queries.GetAll;

public sealed class GetAllSignonSessionsQueryHandler(
    ISignonSessionRepository repository)
    : IRequestHandler<GetAllSignonSessionsQuery, Result<IReadOnlyList<SignonSessionDto>>>
{
    public async Task<Result<IReadOnlyList<SignonSessionDto>>> Handle(
        GetAllSignonSessionsQuery query,
        CancellationToken cancellationToken)
    {
        var entities = await repository.GetAllAsync();

        var dtos = entities
            .Select(e => new SignonSessionDto(
                e.Id,
                e.ImportId,
                e.StatusCode,
                e.StatusSeverity,
                e.ServerDateRaw,
                e.ServerDate,
                e.Language,
                e.CreatedAt,
                e.UpdatedAt))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<SignonSessionDto>>(dtos);
    }
}
