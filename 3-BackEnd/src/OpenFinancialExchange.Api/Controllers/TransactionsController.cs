using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenFinancialExchange.Api.Common;
using OpenFinancialExchange.Application.Transactions.Commands.Create;
using OpenFinancialExchange.Application.Transactions.Commands.Delete;
using OpenFinancialExchange.Application.Transactions.Commands.Reconcile;
using OpenFinancialExchange.Application.Transactions.Commands.Update;
using OpenFinancialExchange.Application.Transactions.Queries.GetAll;
using OpenFinancialExchange.Application.Transactions.Queries.GetByCategoryId;
using OpenFinancialExchange.Application.Transactions.Queries.GetByDateRange;
using OpenFinancialExchange.Application.Transactions.Queries.GetById;
using OpenFinancialExchange.Application.Transactions.Queries.GetByStatementId;
using OpenFinancialExchange.Application.Transactions.Queries.GetUnreconciled;

namespace OpenFinancialExchange.Api.Controllers;

public sealed class TransactionsController(IMediator mediator)
    : BaseApiController(mediator)
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => FromResult(await Mediator.Send(new GetAllTransactionsQuery(), ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => FromResult(await Mediator.Send(new GetByIdTransactionQuery(id), ct));

    [HttpGet("by-statement/{statementId:int}")]
    public async Task<IActionResult> GetByStatementId(int statementId, CancellationToken ct)
        => FromResult(await Mediator.Send(new GetTransactionsByStatementIdQuery(statementId), ct));

    [HttpGet("by-category/{categoryId:int}")]
    public async Task<IActionResult> GetByCategoryId(int categoryId, CancellationToken ct)
        => FromResult(await Mediator.Send(new GetTransactionsByCategoryIdQuery(categoryId), ct));

    [HttpGet("by-date-range")]
    public async Task<IActionResult> GetByDateRange(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken ct)
        => FromResult(await Mediator.Send(new GetTransactionsByDateRangeQuery(from, to), ct));

    [HttpGet("unreconciled")]
    public async Task<IActionResult> GetUnreconciled(CancellationToken ct)
        => FromResult(await Mediator.Send(new GetUnreconciledTransactionsQuery(), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateTransactionCommand(
                request.StatementId,
                request.CategoryId,
                request.TransactionType,
                request.PostedDateRaw,
                request.PostedDate,
                request.TimeZone,
                request.Amount,
                request.FITID,
                request.CheckNumber,
                request.Memo,
                request.PayeeName,
                request.TransactionDateMemo,
                request.OperationSubtype),
            ct);

        if (result.IsFailure) return FromResult(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTransactionRequest request, CancellationToken ct)
        => FromResult(await Mediator.Send(
            new UpdateTransactionCommand(
                id,
                request.CategoryId,
                request.Memo,
                request.PayeeName,
                request.OperationSubtype),
            ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => FromResult(await Mediator.Send(new DeleteTransactionCommand(id), ct));

    [HttpPost("{id:int}/reconcile")]
    public async Task<IActionResult> Reconcile(int id, CancellationToken ct)
        => FromResult(await Mediator.Send(new ReconcileTransactionCommand(id), ct));
}

public sealed record CreateTransactionRequest(
    int      StatementId,
    int?     CategoryId,
    string   TransactionType,
    string   PostedDateRaw,
    DateOnly PostedDate,
    string?  TimeZone,
    decimal  Amount,
    string   FITID,
    string?  CheckNumber,
    string?  Memo,
    string?  PayeeName,
    string?  TransactionDateMemo,
    string?  OperationSubtype);

public sealed record UpdateTransactionRequest(
    int?    CategoryId,
    string? Memo,
    string? PayeeName,
    string? OperationSubtype);
