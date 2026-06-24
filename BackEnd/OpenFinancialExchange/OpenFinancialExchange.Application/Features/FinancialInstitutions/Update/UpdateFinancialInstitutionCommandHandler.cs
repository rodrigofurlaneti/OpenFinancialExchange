using OpenFinancialExchange.Application.Abstractions.Messaging;
using OpenFinancialExchange.Domain.Primitives;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Application.Features.FinancialInstitutions.Update;

internal sealed class UpdateFinancialInstitutionCommandHandler(
    IFinancialInstitutionRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateFinancialInstitutionCommand>
{
    public async Task<Result> Handle(UpdateFinancialInstitutionCommand request, CancellationToken cancellationToken)
    {
        var institution = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (institution is null)
            return Result.Failure(new Error("FinancialInstitution.NotFound",
                $"Financial institution with Id '{request.Id}' was not found."));

        var result = institution.UpdateDetails(request.OrgName, request.Fid);
        if (result.IsFailure)
            return result;

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
