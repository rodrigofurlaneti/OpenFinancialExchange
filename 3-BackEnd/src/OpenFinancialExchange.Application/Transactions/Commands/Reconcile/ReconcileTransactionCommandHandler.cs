using MediatR;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.Transactions.Commands.Reconcile;

public sealed class ReconcileTransactionCommandHandler(
    ITransactionRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ReconcileTransactionCommand, Result>
{
    public async Task<Result> Handle(
        ReconcileTransactionCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(command.Id);
        if (entity is null)
            return Result.Failure(Error.NotFound);

        var result = entity.Reconcile();

        if (result.IsFailure)
            return result;

        await repository.UpdateAsync(entity);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
