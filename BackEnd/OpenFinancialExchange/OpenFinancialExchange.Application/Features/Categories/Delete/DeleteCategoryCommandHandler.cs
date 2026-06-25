using OpenFinancialExchange.Application.Abstractions.Messaging;
using OpenFinancialExchange.Domain.Primitives;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Application.Features.Categories.Delete;

internal sealed class DeleteCategoryCommandHandler(
    ICategoryRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCategoryCommand>
{
    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
            return Result.Failure(new Error("Category.NotFound",
                $"Category with Id '{request.Id}' was not found."));

        var result = category.Deactivate();
        if (result.IsFailure)
            return result;

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
