namespace OpenFinancialExchange.Application.Features.Dashboard;

public sealed record FinancialSummaryResponse(
    decimal TotalCredits,
    decimal TotalDebits,
    decimal NetBalance,
    decimal InternalCredits,
    decimal InternalDebits,
    int TransactionCount,
    IReadOnlyList<TypeSummaryItem> ByType,
    IReadOnlyList<CategorySummaryItem> ByCategory,
    DateTime From,
    DateTime To);

public sealed record TypeSummaryItem(
    string TrnType,
    decimal Total,
    int Count);

public sealed record CategorySummaryItem(
    long? CategoryId,
    string CategoryName,
    string? Color,
    bool IsInternal,
    decimal Credit,
    decimal Debit,
    int Count);
