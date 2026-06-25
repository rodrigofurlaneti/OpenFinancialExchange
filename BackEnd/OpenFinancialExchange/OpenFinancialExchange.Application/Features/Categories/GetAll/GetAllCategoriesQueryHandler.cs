using OpenFinancialExchange.Application.Abstractions.Messaging;
using OpenFinancialExchange.Domain.Primitives;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Application.Features.Categories.GetAll;

internal sealed class GetAllCategoriesQueryHandler(ICategoryRepository repository)
    : IQueryHandler<GetAllCategoriesQuery, IReadOnlyCollection<CategoryResponse>>
{
    public async Task<Result<IReadOnlyCollection<CategoryResponse>>> Handle(
        GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await repository.GetAllAsync(cancellationToken);

        var response = categories
            .Select(c => new CategoryResponse(
                c.Id, c.Name, c.Kind, c.Color, c.IsSystem, c.IsInternal, c.Keywords, c.IsActive, c.CreatedAt, c.UpdatedAt))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyCollection<CategoryResponse>>(response);
    }
}
