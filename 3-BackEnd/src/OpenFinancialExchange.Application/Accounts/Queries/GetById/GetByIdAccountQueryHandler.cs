using MediatR;
using OpenFinancialExchange.Application.Accounts.DTOs;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.Accounts.Queries.GetById;

public sealed class GetByIdAccountQueryHandler(
    IAccountRepository repository)
    : IRequestHandler<GetByIdAccountQuery, Result<AccountDto>>
{
    public async Task<Result<AccountDto>> Handle(
        GetByIdAccountQuery query,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(query.Id);
        if (entity is null)
            return Result.Failure<AccountDto>(Error.NotFound);

        var dto = new AccountDto(
            entity.Id,
            entity.ImportId,
            entity.BankId,
            entity.BranchNumber,
            entity.AccountNumber,
            entity.AccountType,
            entity.DefaultCurrency,
            entity.CreatedAt,
            entity.UpdatedAt);

        return Result.Success(dto);
    }
}
