using OpenFinancialExchange.Application.Abstractions.Messaging;
using OpenFinancialExchange.Domain.Primitives;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Application.Features.Categories.Update;

internal sealed class UpdateCategoryCommandHandler(
    ICategoryRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCategoryCommand>
{
    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
            return Result.Failure(new Error("Category.NotFound",
                $"Category with Id '{request.Id}' was not found."));

        var nameTaken = await repository.ExistsByNameAsync(request.Name.Trim(), request.Id, cancellationToken);
        if (nameTaken)
            return Result.Failure(new Error("Category.AlreadyExists",
                $"A category named '{request.Name}' already exists."));

        var result = category.Update(request.Name, request.Kind, request.Color);
        if (result.IsFailure)
            return result;

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
