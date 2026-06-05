using MediatR;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.SignonSessions.Commands.Update;

public sealed class UpdateSignonSessionCommandHandler(
    ISignonSessionRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSignonSessionCommand, Result>
{
    public async Task<Result> Handle(
        UpdateSignonSessionCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(command.Id);
        if (entity is null)
            return Result.Failure(Error.NotFound);

        var result = entity.Update(
            command.StatusCode,
            command.StatusSeverity,
            command.Language);

        if (result.IsFailure)
            return result;

        await repository.UpdateAsync(entity);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
