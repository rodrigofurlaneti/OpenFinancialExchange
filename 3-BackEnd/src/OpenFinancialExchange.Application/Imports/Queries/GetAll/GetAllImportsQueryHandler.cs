using MediatR;
using OpenFinancialExchange.Application.Imports.DTOs;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.Imports.Queries.GetAll;

public sealed class GetAllImportsQueryHandler(
    IImportRepository repository)
    : IRequestHandler<GetAllImportsQuery, Result<IReadOnlyList<ImportDto>>>
{
    public async Task<Result<IReadOnlyList<ImportDto>>> Handle(
        GetAllImportsQuery query,
        CancellationToken cancellationToken)
    {
        var entities = await repository.GetAllAsync();

        var dtos = entities
            .Select(e => new ImportDto(
                e.Id,
                e.FileName,
                e.ImportedAt,
                e.OFXHeader,
                e.OFXData,
                e.OFXVersion,
                e.OFXSecurity,
                e.OFXEncoding,
                e.OFXCharset,
                e.OFXCompression,
                e.OFXOldFileUID,
                e.OFXNewFileUID,
                e.Notes,
                e.ImportedBy,
                e.CreatedAt,
                e.UpdatedAt))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<ImportDto>>(dtos);
    }
}
