using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenFinancialExchange.Application.Features.Auth.Login;
using OpenFinancialExchange.Application.Features.Auth.Register;

namespace OpenFinancialExchange.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    /// <summary>Autentica um usuário e retorna o token JWT.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginCommand), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsFailure
            ? Unauthorized(new { result.Error.Code, result.Error.Message })
            : Ok(result.Value);
    }

    /// <summary>Registra um novo usuário.</summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code == "Auth.EmailInUse"
                ? Conflict(new { result.Error.Code, result.Error.Message })
                : BadRequest(new { result.Error.Code, result.Error.Message });
        }

        return CreatedAtAction(nameof(Login), result.Value);
    }
}
