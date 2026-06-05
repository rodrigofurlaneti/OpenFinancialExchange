using MediatR;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.TransactionCategories.Commands.Delete;

public sealed class DeleteTransactionCategoryCommandHandler(
    ITransactionCategoryRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteTransactionCategoryCommand, Result>
{
    public async Task<Result> Handle(
        DeleteTransactionCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(command.Id);
        if (entity is null)
            return Result.Failure(Error.NotFound);

        await repository.DeleteAsync(entity);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
