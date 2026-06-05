using MediatR;
using OpenFinancialExchange.Application.Transactions.DTOs;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.Transactions.Queries.GetByCategoryId;

public sealed class GetTransactionsByCategoryIdQueryHandler(
    ITransactionRepository repository)
    : IRequestHandler<GetTransactionsByCategoryIdQuery, Result<IReadOnlyList<TransactionDto>>>
{
    public async Task<Result<IReadOnlyList<TransactionDto>>> Handle(
        GetTransactionsByCategoryIdQuery query,
        CancellationToken cancellationToken)
    {
        var entities = await repository.GetByCategoryIdAsync(query.CategoryId, cancellationToken);

        var dtos = entities
            .Select(e => new TransactionDto(
                e.Id,
                e.StatementId,
                e.CategoryId,
                e.TransactionType,
                e.PostedDateRaw,
                e.PostedDate,
                e.TimeZone,
                e.Amount,
                e.FITID,
                e.CheckNumber,
                e.Memo,
                e.AbsoluteAmount,
                e.MovementNature,
                e.PayeeName,
                e.TransactionDateMemo,
                e.OperationSubtype,
                e.IsReconciled,
                e.ReconciledAt,
                e.CreatedAt,
                e.UpdatedAt))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<TransactionDto>>(dtos);
    }
}
