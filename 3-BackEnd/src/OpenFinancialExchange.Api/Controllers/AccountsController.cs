using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenFinancialExchange.Api.Common;
using OpenFinancialExchange.Application.Accounts.Commands.Create;
using OpenFinancialExchange.Application.Accounts.Commands.Delete;
using OpenFinancialExchange.Application.Accounts.Commands.Update;
using OpenFinancialExchange.Application.Accounts.Queries.GetAll;
using OpenFinancialExchange.Application.Accounts.Queries.GetByBankId;
using OpenFinancialExchange.Application.Accounts.Queries.GetById;
using OpenFinancialExchange.Application.Accounts.Queries.GetByImportId;

namespace OpenFinancialExchange.Api.Controllers;

public sealed class AccountsController(IMediator mediator)
    : BaseApiController(mediator)
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => FromResult(await Mediator.Send(new GetAllAccountsQuery(), ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => FromResult(await Mediator.Send(new GetByIdAccountQuery(id), ct));

    [HttpGet("by-import/{importId:int}")]
    public async Task<IActionResult> GetByImportId(int importId, CancellationToken ct)
        => FromResult(await Mediator.Send(new GetAccountsByImportIdQuery(importId), ct));

    [HttpGet("by-bank/{bankId:int}")]
    public async Task<IActionResult> GetByBankId(int bankId, CancellationToken ct)
        => FromResult(await Mediator.Send(new GetAccountsByBankIdQuery(bankId), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAccountRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateAccountCommand(
                request.ImportId,
                request.BankId,
                request.BranchNumber,
                request.AccountNumber,
                request.AccountType,
                request.DefaultCurrency),
            ct);

        if (result.IsFailure) return FromResult(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAccountRequest request, CancellationToken ct)
        => FromResult(await Mediator.Send(
            new UpdateAccountCommand(
                id,
                request.BranchNumber,
                request.AccountNumber,
                request.AccountType,
                request.DefaultCurrency),
            ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => FromResult(await Mediator.Send(new DeleteAccountCommand(id), ct));
}

internal sealed record CreateAccountRequest(
    int     ImportId,
    int     BankId,
    string? BranchNumber,
    string  AccountNumber,
    string  AccountType,
    string  DefaultCurrency);

internal sealed record UpdateAccountRequest(
    string? BranchNumber,
    string  AccountNumber,
    string  AccountType,
    string  DefaultCurrency);
