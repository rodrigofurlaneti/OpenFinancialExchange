using MediatR;
using OpenFinancialExchange.Application.Imports.DTOs;
using OpenFinancialExchange.Domain.Common;
using OpenFinancialExchange.Domain.Interfaces.Repositories;

namespace OpenFinancialExchange.Application.Imports.Queries.GetById;

public sealed class GetByIdImportQueryHandler(
    IImportRepository repository)
    : IRequestHandler<GetByIdImportQuery, Result<ImportDto>>
{
    public async Task<Result<ImportDto>> Handle(
        GetByIdImportQuery query,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(query.Id);
        if (entity is null)
            return Result.Failure<ImportDto>(Error.NotFound);

        var dto = new ImportDto(
            entity.Id,
            entity.FileName,
            entity.ImportedAt,
            entity.OFXHeader,
            entity.OFXData,
            entity.OFXVersion,
            entity.OFXSecurity,
            entity.OFXEncoding,
            entity.OFXCharset,
            entity.OFXCompression,
            entity.OFXOldFileUID,
            entity.OFXNewFileUID,
            entity.Notes,
            entity.ImportedBy,
            entity.CreatedAt,
            entity.UpdatedAt);

        return Result.Success(dto);
    }
}
