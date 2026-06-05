using MediatR;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Interfaces;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.Imports.Commands.Create;

public sealed class CreateImportCommandHandler(
    IImportRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateImportCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateImportCommand command,
        CancellationToken cancellationToken)
    {
        var result = Import.Create(
            command.FileName,
            command.OFXHeader,
            command.OFXData,
            command.OFXVersion,
            command.OFXSecurity,
            command.OFXEncoding,
            command.OFXCharset,
            command.OFXCompression,
            command.OFXOldFileUID,
            command.OFXNewFileUID,
            command.Notes,
            command.ImportedBy);

        if (result.IsFailure)
            return Result.Failure<int>(result.Error);

        await repository.AddAsync(result.Value);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}
