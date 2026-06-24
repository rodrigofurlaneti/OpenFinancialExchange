using OpenFinancialExchange.Application.Abstractions.Messaging;
using OpenFinancialExchange.Domain.Primitives;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Application.Features.BankAccounts.GetById;

internal sealed class GetBankAccountByIdQueryHandler(IBankAccountRepository repository)
    : IQueryHandler<GetBankAccountByIdQuery, BankAccountResponse>
{
    public async Task<Result<BankAccountResponse>> Handle(
        GetBankAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (account is null)
            return Result.Failure<BankAccountResponse>(new Error("BankAccount.NotFound",
                $"Bank account with Id '{request.Id}' was not found."));

        return Result.Success(new BankAccountResponse(
            account.Id, account.FinancialInstitutionId, account.BankId, account.BranchId,
            account.AcctId, account.AcctType, account.IsActive, account.CreatedAt, account.UpdatedAt));
    }
}
