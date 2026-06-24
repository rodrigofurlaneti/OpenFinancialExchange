using OpenFinancialExchange.Application.Abstractions.Messaging;
using OpenFinancialExchange.Domain.Primitives;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Application.Features.FinancialInstitutions.GetById;

internal sealed class GetFinancialInstitutionByIdQueryHandler(IFinancialInstitutionRepository repository)
    : IQueryHandler<GetFinancialInstitutionByIdQuery, FinancialInstitutionResponse>
{
    public async Task<Result<FinancialInstitutionResponse>> Handle(
        GetFinancialInstitutionByIdQuery request, CancellationToken cancellationToken)
    {
        var institution = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (institution is null)
            return Result.Failure<FinancialInstitutionResponse>(new Error("FinancialInstitution.NotFound",
                $"Financial institution with Id '{request.Id}' was not found."));

        return Result.Success(new FinancialInstitutionResponse(
            institution.Id, institution.BankId, institution.OrgName, institution.Fid,
            institution.IsActive, institution.CreatedAt, institution.UpdatedAt));
    }
}
