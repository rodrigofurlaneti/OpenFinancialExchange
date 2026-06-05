using MediatR;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.Imports.Commands.Update;

public sealed class UpdateImportCommandHandler(
    IImportRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateImportCommand, Result>
{
    public async Task<Result> Handle(
        UpdateImportCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(command.Id);
        if (entity is null)
            return Result.Failure(Error.NotFound);

        var result = entity.UpdateNotes(command.Notes);

        if (result.IsFailure)
            return result;

        await repository.UpdateAsync(entity);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
