using MediatR;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.LedgerBalances.Commands.Delete;

public sealed class DeleteLedgerBalanceCommandHandler(
    ILedgerBalanceRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteLedgerBalanceCommand, Result>
{
    public async Task<Result> Handle(
        DeleteLedgerBalanceCommand command,
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
