using MediatR;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.Banks.Commands.Update;

public sealed class UpdateBankCommandHandler(
    IBankRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateBankCommand, Result>
{
    public async Task<Result> Handle(
        UpdateBankCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(command.Id);
        if (entity is null)
            return Result.Failure(Error.NotFound);

        var result = entity.Update(command.BankName, command.ISPB);

        if (result.IsFailure)
            return result;

        await repository.UpdateAsync(entity);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
