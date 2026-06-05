using MediatR;
using OpenFinancialExchange.Application.TransactionCategories.DTOs;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.TransactionCategories.Queries.GetById;

public sealed class GetByIdTransactionCategoryQueryHandler(
    ITransactionCategoryRepository repository)
    : IRequestHandler<GetByIdTransactionCategoryQuery, Result<TransactionCategoryDto>>
{
    public async Task<Result<TransactionCategoryDto>> Handle(
        GetByIdTransactionCategoryQuery query,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(query.Id);
        if (entity is null)
            return Result.Failure<TransactionCategoryDto>(Error.NotFound);

        var dto = new TransactionCategoryDto(
            entity.Id,
            entity.Code,
            entity.Description,
            entity.OperationType,
            entity.AccountingNature,
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt);

        return Result.Success(dto);
    }
}
