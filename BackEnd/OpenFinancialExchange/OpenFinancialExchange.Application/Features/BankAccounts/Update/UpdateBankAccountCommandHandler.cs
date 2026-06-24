using OpenFinancialExchange.Application.Abstractions.Messaging;
using OpenFinancialExchange.Domain.Primitives;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Application.Features.BankAccounts.Update;

internal sealed class UpdateBankAccountCommandHandler(
    IBankAccountRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateBankAccountCommand>
{
    public async Task<Result> Handle(UpdateBankAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (account is null)
            return Result.Failure(new Error("BankAccount.NotFound",
                $"Bank account with Id '{request.Id}' was not found."));

        var result = account.UpdateDetails(request.BranchId, request.AcctType);
        if (result.IsFailure)
            return result;

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
