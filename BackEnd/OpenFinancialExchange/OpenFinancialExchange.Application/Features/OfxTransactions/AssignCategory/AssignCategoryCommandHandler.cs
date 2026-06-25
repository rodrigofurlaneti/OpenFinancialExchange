using OpenFinancialExchange.Application.Abstractions.Messaging;
using OpenFinancialExchange.Domain.Primitives;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Application.Features.OfxTransactions.AssignCategory;

internal sealed class AssignCategoryCommandHandler(
    IOfxTransactionRepository transactionRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AssignCategoryCommand>
{
    public async Task<Result> Handle(AssignCategoryCommand request, CancellationToken cancellationToken)
    {
        var transaction = await transactionRepository.GetByIdForUpdateAsync(request.TransactionId, cancellationToken);
        if (transaction is null)
            return Result.Failure(new Error("OfxTransaction.NotFound",
                $"Transaction with Id '{request.TransactionId}' was not found."));

        if (request.CategoryId is { } categoryId)
        {
            var category = await categoryRepository.GetByIdAsync(categoryId, cancellationToken);
            if (category is null)
                return Result.Failure(new Error("Category.NotFound",
                    $"Category with Id '{categoryId}' was not found."));
        }

        transaction.AssignCategory(request.CategoryId);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
