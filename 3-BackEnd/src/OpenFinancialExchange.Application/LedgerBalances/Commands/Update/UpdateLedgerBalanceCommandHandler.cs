using MediatR;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.LedgerBalances.Commands.Update;

public sealed class UpdateLedgerBalanceCommandHandler(
    ILedgerBalanceRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateLedgerBalanceCommand, Result>
{
    public async Task<Result> Handle(
        UpdateLedgerBalanceCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(command.Id);
        if (entity is null)
            return Result.Failure(Error.NotFound);

        var result = entity.Update(command.BalanceType, command.Amount, command.AsOfDate);

        if (result.IsFailure)
            return result;

        await repository.UpdateAsync(entity);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
