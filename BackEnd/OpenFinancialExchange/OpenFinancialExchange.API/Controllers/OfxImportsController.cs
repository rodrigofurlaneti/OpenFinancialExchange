using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenFinancialExchange.Application.Features.OfxImports.Create;
using OpenFinancialExchange.Application.Features.OfxImports.GetAll;
using OpenFinancialExchange.Application.Features.OfxImports.GetById;
using OpenFinancialExchange.Application.Features.OfxImports.Reprocess;

namespace OpenFinancialExchange.API.Controllers;

[Authorize]
public sealed class OfxImportsController(IMediator mediator) : ApiController(mediator)
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetAllOfxImportsQuery(), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetOfxImportByIdQuery(id), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOfxImportCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsFailure
            ? HandleFailure(result)
            : CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    /// <summary>Re-parses one stored import and recreates its statement/transactions.</summary>
    [HttpPost("{id:long}/reprocess")]
    public async Task<IActionResult> Reprocess(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ReprocessImportCommand(id), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(new { transactionsCreated = result.Value });
    }

    /// <summary>Reprocesses every stored import for the current user.</summary>
    [HttpPost("reprocess-all")]
    public async Task<IActionResult> ReprocessAll(CancellationToken ct)
    {
        var result = await Mediator.Send(new ReprocessAllImportsCommand(), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }
}
