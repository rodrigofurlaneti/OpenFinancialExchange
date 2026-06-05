using MediatR;
using OpenFinancialExchange.Application.LedgerBalances.DTOs;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.LedgerBalances.Queries.GetById;

public sealed class GetByIdLedgerBalanceQueryHandler(
    ILedgerBalanceRepository repository)
    : IRequestHandler<GetByIdLedgerBalanceQuery, Result<LedgerBalanceDto>>
{
    public async Task<Result<LedgerBalanceDto>> Handle(
        GetByIdLedgerBalanceQuery query,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(query.Id);
        if (entity is null)
            return Result.Failure<LedgerBalanceDto>(Error.NotFound);

        var dto = new LedgerBalanceDto(
            entity.Id,
            entity.StatementId,
            entity.BalanceType,
            entity.Amount,
            entity.AsOfDate,
            entity.CreatedAt,
            entity.UpdatedAt);

        return Result.Success(dto);
    }
}
