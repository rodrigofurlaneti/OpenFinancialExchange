using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenFinancialExchange.Domain.Primitives;

namespace OpenFinancialExchange.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiController(IMediator mediator) : ControllerBase
{
    protected readonly IMediator Mediator = mediator;

    protected IActionResult HandleFailure(Result result)
        => result.Error.Code switch
        {
            var code when code.EndsWith(".NotFound") => NotFound(CreateProblemDetails(result)),
            var code when code.EndsWith(".AlreadyExists") => Conflict(CreateProblemDetails(result)),
            var code when code.EndsWith(".Duplicate") => Conflict(CreateProblemDetails(result)),
            _ => BadRequest(CreateProblemDetails(result))
        };

    private static ProblemDetails CreateProblemDetails(Result result)
        => new()
        {
            Title = result.Error.Code,
            Detail = result.Error.Message
        };
}
