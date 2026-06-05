using MediatR;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Interfaces;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.LedgerBalances.Commands.Create;

public sealed class CreateLedgerBalanceCommandHandler(
    ILedgerBalanceRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateLedgerBalanceCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateLedgerBalanceCommand command,
        CancellationToken cancellationToken)
    {
        var result = LedgerBalance.Create(
            command.StatementId,
            command.BalanceType,
            command.Amount,
            command.AsOfDate);

        if (result.IsFailure)
            return Result.Failure<int>(result.Error);

        await repository.AddAsync(result.Value);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}
