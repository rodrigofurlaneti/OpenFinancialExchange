using MediatR;
using OpenFinancialExchange.Application.Transactions.DTOs;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.Transactions.Queries.GetByDateRange;

public sealed class GetTransactionsByDateRangeQueryHandler(
    ITransactionRepository repository)
    : IRequestHandler<GetTransactionsByDateRangeQuery, Result<IReadOnlyList<TransactionDto>>>
{
    public async Task<Result<IReadOnlyList<TransactionDto>>> Handle(
        GetTransactionsByDateRangeQuery query,
        CancellationToken cancellationToken)
    {
        if (query.From > query.To)
            return Result.Failure<IReadOnlyList<TransactionDto>>(
                new Error("Transaction.InvalidDateRange", "From date must not be after To date."));

        var entities = await repository.GetByDateRangeAsync(query.From, query.To);

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
