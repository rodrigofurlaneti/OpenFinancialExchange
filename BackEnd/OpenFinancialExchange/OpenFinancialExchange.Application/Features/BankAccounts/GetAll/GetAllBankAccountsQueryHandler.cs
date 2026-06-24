using OpenFinancialExchange.Application.Abstractions.Messaging;
using OpenFinancialExchange.Domain.Primitives;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Application.Features.BankAccounts.GetAll;

internal sealed class GetAllBankAccountsQueryHandler(IBankAccountRepository repository)
    : IQueryHandler<GetAllBankAccountsQuery, IReadOnlyCollection<BankAccountResponse>>
{
    public async Task<Result<IReadOnlyCollection<BankAccountResponse>>> Handle(
        GetAllBankAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await repository.GetAllAsync(cancellationToken);
        var response = accounts
            .Select(a => new BankAccountResponse(
                a.Id, a.FinancialInstitutionId, a.BankId, a.BranchId,
                a.AcctId, a.AcctType, a.IsActive, a.CreatedAt, a.UpdatedAt))
            .ToList().AsReadOnly();
        return Result.Success<IReadOnlyCollection<BankAccountResponse>>(response);
    }
}
