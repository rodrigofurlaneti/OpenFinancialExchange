namespace OpenFinancialExchange.Application.TransactionCategories.DTOs;

public sealed record TransactionCategoryDto(
    int Id,
    string Code,
    string Description,
    string OperationType,
    string AccountingNature,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
