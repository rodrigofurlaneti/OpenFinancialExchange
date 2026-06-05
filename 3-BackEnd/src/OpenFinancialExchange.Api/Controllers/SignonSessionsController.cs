using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenFinancialExchange.Api.Common;
using OpenFinancialExchange.Application.SignonSessions.Commands.Create;
using OpenFinancialExchange.Application.SignonSessions.Commands.Delete;
using OpenFinancialExchange.Application.SignonSessions.Commands.Update;
using OpenFinancialExchange.Application.SignonSessions.Queries.GetAll;
using OpenFinancialExchange.Application.SignonSessions.Queries.GetById;
using OpenFinancialExchange.Application.SignonSessions.Queries.GetByImportId;

namespace OpenFinancialExchange.Api.Controllers;

public sealed class SignonSessionsController(IMediator mediator)
    : BaseApiController(mediator)
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => FromResult(await Mediator.Send(new GetAllSignonSessionsQuery(), ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => FromResult(await Mediator.Send(new GetByIdSignonSessionQuery(id), ct));

    [HttpGet("by-import/{importId:int}")]
    public async Task<IActionResult> GetByImportId(int importId, CancellationToken ct)
        => FromResult(await Mediator.Send(new GetSignonSessionByImportIdQuery(importId), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSignonSessionRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateSignonSessionCommand(
                request.ImportId,
                request.StatusCode,
                request.StatusSeverity,
                request.ServerDateRaw,
                request.ServerDate,
                request.Language),
            ct);

        if (result.IsFailure) return FromResult(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSignonSessionRequest request, CancellationToken ct)
        => FromResult(await Mediator.Send(
            new UpdateSignonSessionCommand(
                id,
                request.StatusCode,
                request.StatusSeverity,
                request.Language),
            ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => FromResult(await Mediator.Send(new DeleteSignonSessionCommand(id), ct));
}

internal sealed record CreateSignonSessionRequest(
    int       ImportId,
    string    StatusCode,
    string    StatusSeverity,
    string    ServerDateRaw,
    DateTime? ServerDate,
    string    Language);

internal sealed record UpdateSignonSessionRequest(
    string StatusCode,
    string StatusSeverity,
    string Language);
