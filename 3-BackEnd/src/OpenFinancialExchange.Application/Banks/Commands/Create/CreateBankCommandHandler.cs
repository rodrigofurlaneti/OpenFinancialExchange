using MediatR;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Interfaces;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.Banks.Commands.Create;

public sealed class CreateBankCommandHandler(
    IBankRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateBankCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateBankCommand command,
        CancellationToken cancellationToken)
    {
        var codeExists = await repository.COMPECodeExistsAsync(command.COMPECode);
        if (codeExists)
            return Result.Failure<int>(Error.Conflict);

        var result = Bank.Create(command.COMPECode, command.BankName, command.ISPB);

        if (result.IsFailure)
            return Result.Failure<int>(result.Error);

        await repository.AddAsync(result.Value);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}
