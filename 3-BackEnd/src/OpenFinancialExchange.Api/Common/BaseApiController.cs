using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Api.Common;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController(IMediator mediator) : ControllerBase
{
    protected IMediator Mediator { get; } = mediator;

    protected IActionResult FromResult<T>(Result<T> result)
        => result.IsSuccess ? Ok(result.Value) : Problem(result.Error);

    protected IActionResult FromResult(Result result)
        => result.IsSuccess ? NoContent() : Problem(result.Error);

    private IActionResult Problem(Error error)
        => error.Code == "General.NotFound"
            ? NotFound(new { error.Code, error.Description })
            : BadRequest(new { error.Code, error.Description });
}
