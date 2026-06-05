using MediatR;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Interfaces;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.Transactions.Commands.Create;

public sealed class CreateTransactionCommandHandler(
    ITransactionRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateTransactionCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateTransactionCommand command,
        CancellationToken cancellationToken)
    {
        var fitidExists = await repository.FITIDExistsAsync(command.FITID);
        if (fitidExists)
            return Result.Failure<int>(Error.Conflict);

        var result = Transaction.Create(
            command.StatementId,
            command.CategoryId,
            command.TransactionType,
            command.PostedDateRaw,
            command.PostedDate,
            command.TimeZone,
            command.Amount,
            command.FITID,
            command.CheckNumber,
            command.Memo,
            command.PayeeName,
            command.TransactionDateMemo,
            command.OperationSubtype);

        if (result.IsFailure)
            return Result.Failure<int>(result.Error);

        await repository.AddAsync(result.Value);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}
