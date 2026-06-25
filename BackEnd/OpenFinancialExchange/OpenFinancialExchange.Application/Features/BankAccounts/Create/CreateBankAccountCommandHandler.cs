using OpenFinancialExchange.Application.Abstractions;
using OpenFinancialExchange.Application.Abstractions.Messaging;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Primitives;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Application.Features.BankAccounts.Create;

internal sealed class CreateBankAccountCommandHandler(
    IBankAccountRepository repository,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateBankAccountCommand, long>
{
    public async Task<Result<long>> Handle(CreateBankAccountCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
            return Result.Failure<long>(new Error("Auth.Unauthorized", "No authenticated user."));

        var exists = await repository.ExistsAsync(request.BankId, request.BranchId, request.AcctId, cancellationToken);
        if (exists)
            return Result.Failure<long>(new Error("BankAccount.AlreadyExists",
                $"A bank account with BankId '{request.BankId}' and AcctId '{request.AcctId}' already exists."));

        var result = BankAccount.Create(userId, request.FinancialInstitutionId, request.BankId,
            request.BranchId, request.AcctId, request.AcctType);
        if (result.IsFailure)
            return Result.Failure<long>(result.Error);

        await repository.AddAsync(result.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}
