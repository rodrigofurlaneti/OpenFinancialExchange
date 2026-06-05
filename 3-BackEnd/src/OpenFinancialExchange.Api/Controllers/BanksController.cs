using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenFinancialExchange.Api.Common;
using OpenFinancialExchange.Application.Banks.Commands.Create;
using OpenFinancialExchange.Application.Banks.Commands.Delete;
using OpenFinancialExchange.Application.Banks.Commands.Update;
using OpenFinancialExchange.Application.Banks.Queries.GetAll;
using OpenFinancialExchange.Application.Banks.Queries.GetById;

namespace OpenFinancialExchange.Api.Controllers;

public sealed class BanksController(IMediator mediator)
    : BaseApiController(mediator)
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => FromResult(await Mediator.Send(new GetAllBanksQuery(), ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => FromResult(await Mediator.Send(new GetByIdBankQuery(id), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBankRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateBankCommand(request.COMPECode, request.BankName, request.ISPB),
            ct);

        if (result.IsFailure) return FromResult(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBankRequest request, CancellationToken ct)
        => FromResult(await Mediator.Send(
            new UpdateBankCommand(id, request.BankName, request.ISPB),
            ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => FromResult(await Mediator.Send(new DeleteBankCommand(id), ct));
}

internal sealed record CreateBankRequest(
    string COMPECode,
    string BankName,
    string? ISPB);

internal sealed record UpdateBankRequest(
    string BankName,
    string? ISPB);
