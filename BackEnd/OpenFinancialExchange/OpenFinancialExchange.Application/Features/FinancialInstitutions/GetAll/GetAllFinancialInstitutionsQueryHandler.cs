using OpenFinancialExchange.Application.Abstractions.Messaging;
using OpenFinancialExchange.Domain.Primitives;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Application.Features.FinancialInstitutions.GetAll;

internal sealed class GetAllFinancialInstitutionsQueryHandler(IFinancialInstitutionRepository repository)
    : IQueryHandler<GetAllFinancialInstitutionsQuery, IReadOnlyCollection<FinancialInstitutionResponse>>
{
    public async Task<Result<IReadOnlyCollection<FinancialInstitutionResponse>>> Handle(
        GetAllFinancialInstitutionsQuery request, CancellationToken cancellationToken)
    {
        var institutions = await repository.GetAllAsync(cancellationToken);

        var response = institutions
            .Select(i => new FinancialInstitutionResponse(
                i.Id, i.BankId, i.OrgName, i.Fid, i.IsActive, i.CreatedAt, i.UpdatedAt))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyCollection<FinancialInstitutionResponse>>(response);
    }
}
