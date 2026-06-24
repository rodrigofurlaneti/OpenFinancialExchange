namespace OpenFinancialExchange.Application.Features.Dashboard;

public sealed record FinancialSummaryResponse(
    decimal TotalCredits,
    decimal TotalDebits,
    decimal NetBalance,
    int TransactionCount,
    IReadOnlyList<TypeSummaryItem> ByType,
    DateTime From,
    DateTime To);

public sealed record TypeSummaryItem(
    string TrnType,
    decimal Total,
    int Count);
