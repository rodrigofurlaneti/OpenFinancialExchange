using MediatR;
using OpenFinancialExchange.Application.Statements.DTOs;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.Statements.Queries.GetAll;

public sealed class GetAllStatementsQueryHandler(
    IStatementRepository repository)
    : IRequestHandler<GetAllStatementsQuery, Result<IReadOnlyList<StatementDto>>>
{
    public async Task<Result<IReadOnlyList<StatementDto>>> Handle(
        GetAllStatementsQuery query,
        CancellationToken cancellationToken)
    {
        var entities = await repository.GetAllAsync();

        var dtos = entities
            .Select(e => new StatementDto(
                e.Id,
                e.AccountId,
                e.TRNUID,
                e.StatusCode,
                e.StatusSeverity,
                e.StartDate,
                e.EndDate,
                e.TimeZone,
                e.CreatedAt,
                e.UpdatedAt))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<StatementDto>>(dtos);
    }
}
