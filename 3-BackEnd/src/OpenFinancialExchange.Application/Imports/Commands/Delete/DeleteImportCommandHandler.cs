using MediatR;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.Imports.Commands.Delete;

public sealed class DeleteImportCommandHandler(
    IImportRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteImportCommand, Result>
{
    public async Task<Result> Handle(
        DeleteImportCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(command.Id);
        if (entity is null)
            return Result.Failure(Error.NotFound);

        await repository.DeleteAsync(entity);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
