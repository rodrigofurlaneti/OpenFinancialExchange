using MediatR;
using OpenFinancialExchange.Application.Accounts.DTOs;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.Accounts.Queries.GetAll;

public sealed class GetAllAccountsQueryHandler(
    IAccountRepository repository)
    : IRequestHandler<GetAllAccountsQuery, Result<IReadOnlyList<AccountDto>>>
{
    public async Task<Result<IReadOnlyList<AccountDto>>> Handle(
        GetAllAccountsQuery query,
        CancellationToken cancellationToken)
    {
        var entities = await repository.GetAllAsync();

        var dtos = entities
            .Select(e => new AccountDto(
                e.Id,
                e.ImportId,
                e.BankId,
                e.BranchNumber,
                e.AccountNumber,
                e.AccountType,
                e.DefaultCurrency,
                e.CreatedAt,
                e.UpdatedAt))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<AccountDto>>(dtos);
    }
}
