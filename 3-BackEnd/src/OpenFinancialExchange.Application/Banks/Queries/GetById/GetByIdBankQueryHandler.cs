using MediatR;
using OpenFinancialExchange.Application.Banks.DTOs;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.Banks.Queries.GetById;

public sealed class GetByIdBankQueryHandler(
    IBankRepository repository)
    : IRequestHandler<GetByIdBankQuery, Result<BankDto>>
{
    public async Task<Result<BankDto>> Handle(
        GetByIdBankQuery query,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(query.Id);
        if (entity is null)
            return Result.Failure<BankDto>(Error.NotFound);

        var dto = new BankDto(
            entity.Id,
            entity.COMPECode,
            entity.BankName,
            entity.ISPB,
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt);

        return Result.Success(dto);
    }
}
