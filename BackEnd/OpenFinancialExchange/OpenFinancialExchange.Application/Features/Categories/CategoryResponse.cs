namespace OpenFinancialExchange.Application.Features.Categories;

public sealed record CategoryResponse(
    long Id,
    string Name,
    string Kind,
    string Color,
    bool IsSystem,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
