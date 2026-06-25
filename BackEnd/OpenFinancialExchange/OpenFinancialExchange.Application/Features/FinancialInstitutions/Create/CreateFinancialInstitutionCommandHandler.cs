using OpenFinancialExchange.Application.Abstractions;
using OpenFinancialExchange.Application.Abstractions.Messaging;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Primitives;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Application.Features.FinancialInstitutions.Create;

internal sealed class CreateFinancialInstitutionCommandHandler(
    IFinancialInstitutionRepository repository,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateFinancialInstitutionCommand, long>
{
    public async Task<Result<long>> Handle(CreateFinancialInstitutionCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
            return Result.Failure<long>(new Error("Auth.Unauthorized", "No authenticated user."));

        var exists = await repository.ExistsAsync(request.BankId, request.Fid, cancellationToken);
        if (exists)
            return Result.Failure<long>(new Error("FinancialInstitution.AlreadyExists",
                $"A financial institution with BankId '{request.BankId}' already exists."));

        var result = FinancialInstitution.Create(userId, request.BankId, request.OrgName, request.Fid);
        if (result.IsFailure)
            return Result.Failure<long>(result.Error);

        await repository.AddAsync(result.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}
