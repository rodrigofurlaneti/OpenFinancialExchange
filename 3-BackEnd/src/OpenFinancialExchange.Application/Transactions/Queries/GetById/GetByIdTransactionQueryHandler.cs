using MediatR;
using OpenFinancialExchange.Application.Transactions.DTOs;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.Transactions.Queries.GetById;

public sealed class GetByIdTransactionQueryHandler(
    ITransactionRepository repository)
    : IRequestHandler<GetByIdTransactionQuery, Result<TransactionDto>>
{
    public async Task<Result<TransactionDto>> Handle(
        GetByIdTransactionQuery query,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(query.Id);
        if (entity is null)
            return Result.Failure<TransactionDto>(Error.NotFound);

        var dto = new TransactionDto(
            entity.Id,
            entity.StatementId,
            entity.CategoryId,
            entity.TransactionType,
            entity.PostedDateRaw,
            entity.PostedDate,
            entity.TimeZone,
            entity.Amount,
            entity.FITID,
            entity.CheckNumber,
            entity.Memo,
            entity.AbsoluteAmount,
            entity.MovementNature,
            entity.PayeeName,
            entity.TransactionDateMemo,
            entity.OperationSubtype,
            entity.IsReconciled,
            entity.ReconciledAt,
            entity.CreatedAt,
            entity.UpdatedAt);

        return Result.Success(dto);
    }
}
