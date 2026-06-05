using MediatR;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.Statements.Commands.Update;

public sealed class UpdateStatementCommandHandler(
    IStatementRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateStatementCommand, Result>
{
    public async Task<Result> Handle(
        UpdateStatementCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(command.Id);
        if (entity is null)
            return Result.Failure(Error.NotFound);

        var result = entity.Update(
            command.StatusCode,
            command.StatusSeverity,
            command.StartDate,
            command.EndDate,
            command.TimeZone);

        if (result.IsFailure)
            return result;

        await repository.UpdateAsync(entity);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
