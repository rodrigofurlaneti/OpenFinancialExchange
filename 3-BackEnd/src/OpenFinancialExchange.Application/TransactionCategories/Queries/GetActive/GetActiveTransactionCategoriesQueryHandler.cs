using MediatR;
using OpenFinancialExchange.Application.TransactionCategories.DTOs;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.TransactionCategories.Queries.GetActive;

public sealed class GetActiveTransactionCategoriesQueryHandler(
    ITransactionCategoryRepository repository)
    : IRequestHandler<GetActiveTransactionCategoriesQuery, Result<IReadOnlyList<TransactionCategoryDto>>>
{
    public async Task<Result<IReadOnlyList<TransactionCategoryDto>>> Handle(
        GetActiveTransactionCategoriesQuery query,
        CancellationToken cancellationToken)
    {
        var entities = await repository.GetActiveAsync();

        var dtos = entities
            .Select(e => new TransactionCategoryDto(
                e.Id,
                e.Code,
                e.Description,
                e.OperationType,
                e.AccountingNature,
                e.IsActive,
                e.CreatedAt,
                e.UpdatedAt))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<TransactionCategoryDto>>(dtos);
    }
}
