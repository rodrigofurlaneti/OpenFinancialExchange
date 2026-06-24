using OpenFinancialExchange.Application.Abstractions.Messaging;
using OpenFinancialExchange.Domain.Primitives;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Application.Features.OfxImports.GetAll;

internal sealed class GetAllOfxImportsQueryHandler(IOfxImportRepository repository)
    : IQueryHandler<GetAllOfxImportsQuery, IReadOnlyCollection<OfxImportResponse>>
{
    public async Task<Result<IReadOnlyCollection<OfxImportResponse>>> Handle(
        GetAllOfxImportsQuery request, CancellationToken cancellationToken)
    {
        var imports = await repository.GetAllAsync(cancellationToken);
        var response = imports
            .Select(i => new OfxImportResponse(i.Id, i.FileName, i.FileHash, i.OfxHeaderVersion,
                i.OfxVersion, i.OfxData, i.Encoding, i.Charset, i.Security, i.ImportedAt))
            .ToList().AsReadOnly();
        return Result.Success<IReadOnlyCollection<OfxImportResponse>>(response);
    }
}
