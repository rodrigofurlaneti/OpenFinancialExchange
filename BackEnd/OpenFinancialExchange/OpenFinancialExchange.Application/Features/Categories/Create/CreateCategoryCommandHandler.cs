using OpenFinancialExchange.Application.Abstractions;
using OpenFinancialExchange.Application.Abstractions.Messaging;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Primitives;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Application.Features.Categories.Create;

internal sealed class CreateCategoryCommandHandler(
    ICategoryRepository repository,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateCategoryCommand, long>
{
    public async Task<Result<long>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
            return Result.Failure<long>(new Error("Auth.Unauthorized", "No authenticated user."));

        var exists = await repository.ExistsByNameAsync(request.Name.Trim(), null, cancellationToken);
        if (exists)
            return Result.Failure<long>(new Error("Category.AlreadyExists",
                $"A category named '{request.Name}' already exists."));

        var result = Category.Create(userId, request.Name, request.Kind, request.Color);
        if (result.IsFailure)
            return Result.Failure<long>(result.Error);

        await repository.AddAsync(result.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}
