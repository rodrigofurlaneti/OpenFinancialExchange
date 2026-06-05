using MediatR;
using OpenFinancialExchange.Application.LedgerBalances.DTOs;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.LedgerBalances.Queries.GetByStatementId;

public sealed class GetLedgerBalancesByStatementIdQueryHandler(
    ILedgerBalanceRepository repository)
    : IRequestHandler<GetLedgerBalancesByStatementIdQuery, Result<IReadOnlyList<LedgerBalanceDto>>>
{
    public async Task<Result<IReadOnlyList<LedgerBalanceDto>>> Handle(
        GetLedgerBalancesByStatementIdQuery query,
        CancellationToken cancellationToken)
    {
        var entities = await repository.GetByStatementIdAsync(query.StatementId, cancellationToken);

        var dtos = entities
            .Select(e => new LedgerBalanceDto(
                e.Id,
                e.StatementId,
                e.BalanceType,
                e.Amount,
                e.AsOfDate,
                e.CreatedAt,
                e.UpdatedAt))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<LedgerBalanceDto>>(dtos);
    }
}
