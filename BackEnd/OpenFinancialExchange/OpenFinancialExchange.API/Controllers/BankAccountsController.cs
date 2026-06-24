using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenFinancialExchange.Application.Features.BankAccounts.Create;
using OpenFinancialExchange.Application.Features.BankAccounts.GetAll;
using OpenFinancialExchange.Application.Features.BankAccounts.GetById;
using OpenFinancialExchange.Application.Features.BankAccounts.Update;

namespace OpenFinancialExchange.API.Controllers;

[Authorize]
public sealed class BankAccountsController(IMediator mediator) : ApiController(mediator)
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetAllBankAccountsQuery(), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetBankAccountByIdQuery(id), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBankAccountCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsFailure
            ? HandleFailure(result)
            : CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateBankAccountRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateBankAccountCommand(id, request.BranchId, request.AcctType), ct);
        return result.IsFailure ? HandleFailure(result) : NoContent();
    }
}

public sealed record UpdateBankAccountRequest(string? BranchId, string AcctType);
