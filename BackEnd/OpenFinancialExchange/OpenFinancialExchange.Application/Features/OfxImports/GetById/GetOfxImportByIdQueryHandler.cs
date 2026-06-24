using OpenFinancialExchange.Application.Abstractions.Messaging;
using OpenFinancialExchange.Domain.Primitives;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Application.Features.OfxImports.GetById;

internal sealed class GetOfxImportByIdQueryHandler(IOfxImportRepository repository)
    : IQueryHandler<GetOfxImportByIdQuery, OfxImportResponse>
{
    public async Task<Result<OfxImportResponse>> Handle(
        GetOfxImportByIdQuery request, CancellationToken cancellationToken)
    {
        var import = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (import is null)
            return Result.Failure<OfxImportResponse>(new Error("OfxImport.NotFound",
                $"OFX import with Id '{request.Id}' was not found."));

        return Result.Success(new OfxImportResponse(import.Id, import.FileName, import.FileHash,
            import.OfxHeaderVersion, import.OfxVersion, import.OfxData, import.Encoding,
            import.Charset, import.Security, import.ImportedAt));
    }
}
