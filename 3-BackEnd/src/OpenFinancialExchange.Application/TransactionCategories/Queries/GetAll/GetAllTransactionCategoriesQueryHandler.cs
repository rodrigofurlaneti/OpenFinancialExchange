using MediatR;
using OpenFinancialExchange.Application.TransactionCategories.DTOs;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.TransactionCategories.Queries.GetAll;

public sealed class GetAllTransactionCategoriesQueryHandler(
    ITransactionCategoryRepository repository)
    : IRequestHandler<GetAllTransactionCategoriesQuery, Result<IReadOnlyList<TransactionCategoryDto>>>
{
    public async Task<Result<IReadOnlyList<TransactionCategoryDto>>> Handle(
        GetAllTransactionCategoriesQuery query,
        CancellationToken cancellationToken)
    {
        var entities = await repository.GetAllAsync();

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
