using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenFinancialExchange.Application.Features.Categories.Create;
using OpenFinancialExchange.Application.Features.Categories.Delete;
using OpenFinancialExchange.Application.Features.Categories.GetAll;
using OpenFinancialExchange.Application.Features.Categories.Update;

namespace OpenFinancialExchange.API.Controllers;

[Authorize]
public sealed class CategoriesController(IMediator mediator) : ApiController(mediator)
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetAllCategoriesQuery(), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsFailure
            ? HandleFailure(result)
            : CreatedAtAction(nameof(GetAll), new { id = result.Value }, result.Value);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateCategoryRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateCategoryCommand(id, request.Name, request.Kind, request.Color), ct);
        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteCategoryCommand(id), ct);
        return result.IsFailure ? HandleFailure(result) : NoContent();
    }
}

public sealed record UpdateCategoryRequest(string Name, string Kind, string Color);
