using MediatR;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Interfaces;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.TransactionCategories.Commands.Create;

public sealed class CreateTransactionCategoryCommandHandler(
    ITransactionCategoryRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateTransactionCategoryCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateTransactionCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var codeExists = await repository.CodeExistsAsync(command.Code);
        if (codeExists)
            return Result.Failure<int>(Error.Conflict);

        var result = TransactionCategory.Create(
            command.Code,
            command.Description,
            command.OperationType,
            command.AccountingNature);

        if (result.IsFailure)
            return Result.Failure<int>(result.Error);

        await repository.AddAsync(result.Value);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}
