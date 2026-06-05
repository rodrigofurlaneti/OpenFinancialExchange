using MediatR;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Interfaces;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.Accounts.Commands.Create;

public sealed class CreateAccountCommandHandler(
    IAccountRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAccountCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateAccountCommand command,
        CancellationToken cancellationToken)
    {
        var result = Account.Create(
            command.ImportId,
            command.BankId,
            command.BranchNumber,
            command.AccountNumber,
            command.AccountType,
            command.DefaultCurrency);

        if (result.IsFailure)
            return Result.Failure<int>(result.Error);

        await repository.AddAsync(result.Value);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}
