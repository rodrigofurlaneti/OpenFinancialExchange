using MediatR;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.Transactions.Commands.Update;

public sealed class UpdateTransactionCommandHandler(
    ITransactionRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateTransactionCommand, Result>
{
    public async Task<Result> Handle(
        UpdateTransactionCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(command.Id);
        if (entity is null)
            return Result.Failure(Error.NotFound);

        var result = entity.Update(
            command.CategoryId,
            command.Memo,
            command.PayeeName,
            command.OperationSubtype);

        if (result.IsFailure)
            return result;

        await repository.UpdateAsync(entity);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
