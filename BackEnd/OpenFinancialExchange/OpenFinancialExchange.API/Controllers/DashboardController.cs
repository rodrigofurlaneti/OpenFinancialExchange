using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenFinancialExchange.Application.Features.Dashboard.GetFinancialSummary;

namespace OpenFinancialExchange.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public sealed class DashboardController(ISender sender) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct)
    {
        if (from > to)
            return BadRequest("'from' must be before 'to'.");

        var result = await sender.Send(new GetFinancialSummaryQuery(from, to), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
