using MediatR;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.TransactionCategories.Commands.Update;

public sealed class UpdateTransactionCategoryCommandHandler(
    ITransactionCategoryRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateTransactionCategoryCommand, Result>
{
    public async Task<Result> Handle(
        UpdateTransactionCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(command.Id);
        if (entity is null)
            return Result.Failure(Error.NotFound);

        var result = entity.Update(
            command.Description,
            command.OperationType,
            command.AccountingNature);

        if (result.IsFailure)
            return result;

        await repository.UpdateAsync(entity);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
