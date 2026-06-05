using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenFinancialExchange.Api.Common;
using OpenFinancialExchange.Application.TransactionCategories.Commands.Create;
using OpenFinancialExchange.Application.TransactionCategories.Commands.Delete;
using OpenFinancialExchange.Application.TransactionCategories.Commands.Update;
using OpenFinancialExchange.Application.TransactionCategories.Queries.GetActive;
using OpenFinancialExchange.Application.TransactionCategories.Queries.GetAll;
using OpenFinancialExchange.Application.TransactionCategories.Queries.GetById;

namespace OpenFinancialExchange.Api.Controllers;

public sealed class TransactionCategoriesController(IMediator mediator)
    : BaseApiController(mediator)
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => FromResult(await Mediator.Send(new GetAllTransactionCategoriesQuery(), ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => FromResult(await Mediator.Send(new GetByIdTransactionCategoryQuery(id), ct));

    [HttpGet("active")]
    public async Task<IActionResult> GetActive(CancellationToken ct)
        => FromResult(await Mediator.Send(new GetActiveTransactionCategoriesQuery(), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionCategoryRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateTransactionCategoryCommand(
                request.Code,
                request.Description,
                request.OperationType,
                request.AccountingNature),
            ct);

        if (result.IsFailure) return FromResult(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTransactionCategoryRequest request, CancellationToken ct)
        => FromResult(await Mediator.Send(
            new UpdateTransactionCategoryCommand(
                id,
                request.Description,
                request.OperationType,
                request.AccountingNature),
            ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => FromResult(await Mediator.Send(new DeleteTransactionCategoryCommand(id), ct));
}

internal sealed record CreateTransactionCategoryRequest(
    string Code,
    string Description,
    string OperationType,
    string AccountingNature);

internal sealed record UpdateTransactionCategoryRequest(
    string Description,
    string OperationType,
    string AccountingNature);
