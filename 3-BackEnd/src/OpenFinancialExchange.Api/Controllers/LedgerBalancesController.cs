using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenFinancialExchange.Api.Common;
using OpenFinancialExchange.Application.LedgerBalances.Commands.Create;
using OpenFinancialExchange.Application.LedgerBalances.Commands.Delete;
using OpenFinancialExchange.Application.LedgerBalances.Commands.Update;
using OpenFinancialExchange.Application.LedgerBalances.Queries.GetAll;
using OpenFinancialExchange.Application.LedgerBalances.Queries.GetById;
using OpenFinancialExchange.Application.LedgerBalances.Queries.GetByStatementId;

namespace OpenFinancialExchange.Api.Controllers;

public sealed class LedgerBalancesController(IMediator mediator)
    : BaseApiController(mediator)
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => FromResult(await Mediator.Send(new GetAllLedgerBalancesQuery(), ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => FromResult(await Mediator.Send(new GetByIdLedgerBalanceQuery(id), ct));

    [HttpGet("by-statement/{statementId:int}")]
    public async Task<IActionResult> GetByStatementId(int statementId, CancellationToken ct)
        => FromResult(await Mediator.Send(new GetLedgerBalancesByStatementIdQuery(statementId), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLedgerBalanceRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateLedgerBalanceCommand(
                request.StatementId,
                request.BalanceType,
                request.Amount,
                request.AsOfDate),
            ct);

        if (result.IsFailure) return FromResult(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateLedgerBalanceRequest request, CancellationToken ct)
        => FromResult(await Mediator.Send(
            new UpdateLedgerBalanceCommand(
                id,
                request.BalanceType,
                request.Amount,
                request.AsOfDate),
            ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => FromResult(await Mediator.Send(new DeleteLedgerBalanceCommand(id), ct));
}

internal sealed record CreateLedgerBalanceRequest(
    int      StatementId,
    string   BalanceType,
    decimal  Amount,
    DateTime AsOfDate);

internal sealed record UpdateLedgerBalanceRequest(
    string   BalanceType,
    decimal  Amount,
    DateTime AsOfDate);
