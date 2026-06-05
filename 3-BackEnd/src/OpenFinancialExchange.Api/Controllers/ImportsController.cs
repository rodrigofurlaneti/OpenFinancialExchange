using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenFinancialExchange.Api.Common;
using OpenFinancialExchange.Application.Imports.Commands.Create;
using OpenFinancialExchange.Application.Imports.Commands.Delete;
using OpenFinancialExchange.Application.Imports.Commands.Update;
using OpenFinancialExchange.Application.Imports.Queries.GetAll;
using OpenFinancialExchange.Application.Imports.Queries.GetById;

namespace OpenFinancialExchange.Api.Controllers;

public sealed class ImportsController(IMediator mediator)
    : BaseApiController(mediator)
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => FromResult(await Mediator.Send(new GetAllImportsQuery(), ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => FromResult(await Mediator.Send(new GetByIdImportQuery(id), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateImportRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateImportCommand(
                request.FileName,
                request.OFXHeader,
                request.OFXData,
                request.OFXVersion,
                request.OFXSecurity,
                request.OFXEncoding,
                request.OFXCharset,
                request.OFXCompression,
                request.OFXOldFileUID,
                request.OFXNewFileUID,
                request.Notes,
                request.ImportedBy),
            ct);

        if (result.IsFailure) return FromResult(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateNotes(int id, [FromBody] UpdateImportRequest request, CancellationToken ct)
        => FromResult(await Mediator.Send(new UpdateImportCommand(id, request.Notes), ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => FromResult(await Mediator.Send(new DeleteImportCommand(id), ct));
}

internal sealed record CreateImportRequest(
    string  FileName,
    string  OFXHeader,
    string  OFXData,
    string  OFXVersion,
    string  OFXSecurity,
    string  OFXEncoding,
    string  OFXCharset,
    string  OFXCompression,
    string  OFXOldFileUID,
    string  OFXNewFileUID,
    string? Notes,
    string? ImportedBy);

internal sealed record UpdateImportRequest(string? Notes);
