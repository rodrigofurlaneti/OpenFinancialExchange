using MediatR;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.Accounts.Commands.Update;

public sealed class UpdateAccountCommandHandler(
    IAccountRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateAccountCommand, Result>
{
    public async Task<Result> Handle(
        UpdateAccountCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(command.Id);
        if (entity is null)
            return Result.Failure(Error.NotFound);

        var result = entity.Update(
            command.BranchNumber,
            command.AccountNumber,
            command.AccountType,
            command.DefaultCurrency);

        if (result.IsFailure)
            return result;

        await repository.UpdateAsync(entity);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
