using MediatR;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Interfaces;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.SignonSessions.Commands.Create;

public sealed class CreateSignonSessionCommandHandler(
    ISignonSessionRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateSignonSessionCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateSignonSessionCommand command,
        CancellationToken cancellationToken)
    {
        var result = SignonSession.Create(
            command.ImportId,
            command.StatusCode,
            command.StatusSeverity,
            command.ServerDateRaw,
            command.ServerDate,
            command.Language);

        if (result.IsFailure)
            return Result.Failure<int>(result.Error);

        await repository.AddAsync(result.Value);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}
