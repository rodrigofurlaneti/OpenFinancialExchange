using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenFinancialExchange.Application.Features.FinancialInstitutions.Create;
using OpenFinancialExchange.Application.Features.FinancialInstitutions.GetAll;
using OpenFinancialExchange.Application.Features.FinancialInstitutions.GetById;
using OpenFinancialExchange.Application.Features.FinancialInstitutions.Update;

namespace OpenFinancialExchange.API.Controllers;

[Authorize]
public sealed class FinancialInstitutionsController(IMediator mediator) : ApiController(mediator)
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetAllFinancialInstitutionsQuery(), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetFinancialInstitutionByIdQuery(id), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFinancialInstitutionCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsFailure
            ? HandleFailure(result)
            : CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateFinancialInstitutionRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateFinancialInstitutionCommand(id, request.OrgName, request.Fid), ct);
        return result.IsFailure ? HandleFailure(result) : NoContent();
    }
}

public sealed record UpdateFinancialInstitutionRequest(string? OrgName, string? Fid);
