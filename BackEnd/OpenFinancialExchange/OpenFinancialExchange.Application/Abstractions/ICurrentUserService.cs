namespace OpenFinancialExchange.Application.Abstractions;

/// <summary>
/// Provides the identity of the authenticated user for the current request.
/// Resolved from the JWT 'sub' claim. Used for multi-tenant data isolation.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>The authenticated user's Id, or null when there is no authenticated user.</summary>
    long? UserId { get; }
}
