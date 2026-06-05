using MediatR;
using OpenFinancialExchange.Application.Banks.DTOs;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.Banks.Queries.GetAll;

public sealed class GetAllBanksQueryHandler(
    IBankRepository repository)
    : IRequestHandler<GetAllBanksQuery, Result<IReadOnlyList<BankDto>>>
{
    public async Task<Result<IReadOnlyList<BankDto>>> Handle(
        GetAllBanksQuery query,
        CancellationToken cancellationToken)
    {
        var entities = await repository.GetAllAsync();

        var dtos = entities
            .Select(e => new BankDto(
                e.Id,
                e.COMPECode,
                e.BankName,
                e.ISPB,
                e.IsActive,
                e.CreatedAt,
                e.UpdatedAt))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<BankDto>>(dtos);
    }
}
