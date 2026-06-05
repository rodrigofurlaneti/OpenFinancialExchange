using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenFinancialExchange.Api.Common;
using OpenFinancialExchange.Application.Statements.Commands.Create;
using OpenFinancialExchange.Application.Statements.Commands.Delete;
using OpenFinancialExchange.Application.Statements.Commands.Update;
using OpenFinancialExchange.Application.Statements.Queries.GetAll;
using OpenFinancialExchange.Application.Statements.Queries.GetByAccountId;
using OpenFinancialExchange.Application.Statements.Queries.GetById;

namespace OpenFinancialExchange.Api.Controllers;

public sealed class StatementsController(IMediator mediator)
    : BaseApiController(mediator)
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => FromResult(await Mediator.Send(new GetAllStatementsQuery(), ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => FromResult(await Mediator.Send(new GetByIdStatementQuery(id), ct));

    [HttpGet("by-account/{accountId:int}")]
    public async Task<IActionResult> GetByAccountId(int accountId, CancellationToken ct)
        => FromResult(await Mediator.Send(new GetStatementsByAccountIdQuery(accountId), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStatementRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateStatementCommand(
                request.AccountId,
                request.TRNUID,
                request.StatusCode,
                request.StatusSeverity,
                request.StartDate,
                request.EndDate,
                request.TimeZone),
            ct);

        if (result.IsFailure) return FromResult(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStatementRequest request, CancellationToken ct)
        => FromResult(await Mediator.Send(
            new UpdateStatementCommand(
                id,
                request.StatusCode,
                request.StatusSeverity,
                request.StartDate,
                request.EndDate,
                request.TimeZone),
            ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => FromResult(await Mediator.Send(new DeleteStatementCommand(id), ct));
}

internal sealed record CreateStatementRequest(
    int       AccountId,
    string    TRNUID,
    string    StatusCode,
    string    StatusSeverity,
    DateTime  StartDate,
    DateTime  EndDate,
    string?   TimeZone);

internal sealed record UpdateStatementRequest(
    string   StatusCode,
    string   StatusSeverity,
    DateTime StartDate,
    DateTime EndDate,
    string?  TimeZone);
