using MediatR;
using OpenFinancialExchange.Application.Statements.DTOs;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.Statements.Queries.GetById;

public sealed class GetByIdStatementQueryHandler(
    IStatementRepository repository)
    : IRequestHandler<GetByIdStatementQuery, Result<StatementDto>>
{
    public async Task<Result<StatementDto>> Handle(
        GetByIdStatementQuery query,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(query.Id);
        if (entity is null)
            return Result.Failure<StatementDto>(Error.NotFound);

        var dto = new StatementDto(
            entity.Id,
            entity.AccountId,
            entity.TRNUID,
            entity.StatusCode,
            entity.StatusSeverity,
            entity.StartDate,
            entity.EndDate,
            entity.TimeZone,
            entity.CreatedAt,
            entity.UpdatedAt);

        return Result.Success(dto);
    }
}
