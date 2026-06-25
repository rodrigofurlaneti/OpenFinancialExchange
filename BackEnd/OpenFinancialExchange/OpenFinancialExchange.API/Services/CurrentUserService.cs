using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using OpenFinancialExchange.Application.Abstractions;

namespace OpenFinancialExchange.API.Services;

/// <summary>
/// Resolves the current user's Id from the JWT 'sub' claim on the active HTTP request.
/// </summary>
internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public long? UserId
    {
        get
        {
            var principal = httpContextAccessor.HttpContext?.User;
            if (principal is null) return null;

            // ASP.NET maps 'sub' to ClaimTypes.NameIdentifier; check both for robustness.
            var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return long.TryParse(value, out var id) ? id : null;
        }
    }
}
