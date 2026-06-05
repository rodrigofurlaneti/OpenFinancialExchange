using MediatR;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Interfaces;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.Statements.Commands.Create;

public sealed class CreateStatementCommandHandler(
    IStatementRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateStatementCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateStatementCommand command,
        CancellationToken cancellationToken)
    {
        var result = Statement.Create(
            command.AccountId,
            command.TRNUID,
            command.StatusCode,
            command.StatusSeverity,
            command.StartDate,
            command.EndDate,
            command.TimeZone);

        if (result.IsFailure)
            return Result.Failure<int>(result.Error);

        await repository.AddAsync(result.Value);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}
