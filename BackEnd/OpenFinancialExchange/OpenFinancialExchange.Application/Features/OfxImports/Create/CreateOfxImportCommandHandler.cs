using OpenFinancialExchange.Application.Abstractions.Messaging;
using OpenFinancialExchange.Domain.Entities;
using OpenFinancialExchange.Domain.Primitives;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Application.Features.OfxImports.Create;

internal sealed class CreateOfxImportCommandHandler(
    IOfxImportRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateOfxImportCommand, long>
{
    public async Task<Result<long>> Handle(CreateOfxImportCommand request, CancellationToken cancellationToken)
    {
        var alreadyImported = await repository.ExistsByHashAsync(request.FileHash, cancellationToken);
        if (alreadyImported)
            return Result.Failure<long>(new Error("OfxImport.Duplicate",
                "This file has already been imported (duplicate SHA-256 hash)."));

        var result = OfxImport.Create(request.FileName, request.FileHash, request.OfxHeaderVersion,
            request.OfxVersion, request.OfxData, request.Encoding, request.Charset,
            request.Security, request.Compression, request.OldFileUid, request.NewFileUid);

        if (result.IsFailure)
            return Result.Failure<long>(result.Error);

        await repository.AddAsync(result.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}
