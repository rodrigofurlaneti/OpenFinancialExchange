using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenFinancialExchange.Application.Features.OfxTransactions.AssignCategory;
using OpenFinancialExchange.Application.Features.OfxTransactions.GetByBankAccount;
using OpenFinancialExchange.Application.Features.OfxTransactions.GetByStatement;

namespace OpenFinancialExchange.API.Controllers;

[Authorize]
public sealed class OfxTransactionsController(IMediator mediator) : ApiController(mediator)
{
    [HttpGet("by-statement/{statementId:long}")]
    public async Task<IActionResult> GetByStatement(long statementId, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetOfxTransactionsByStatementQuery(statementId), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpGet("by-account/{bankAccountId:long}")]
    public async Task<IActionResult> GetByBankAccount(
        long bankAccountId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new GetOfxTransactionsByBankAccountQuery(bankAccountId, from, to), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpPatch("{id:long}/category")]
    public async Task<IActionResult> AssignCategory(long id, [FromBody] AssignCategoryRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new AssignCategoryCommand(id, request.CategoryId), ct);
        return result.IsFailure ? HandleFailure(result) : NoContent();
    }
}

public sealed record AssignCategoryRequest(long? CategoryId);
