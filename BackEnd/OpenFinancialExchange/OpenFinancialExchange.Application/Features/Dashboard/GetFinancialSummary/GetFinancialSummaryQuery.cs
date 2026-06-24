using OpenFinancialExchange.Application.Abstractions.Messaging;

namespace OpenFinancialExchange.Application.Features.Dashboard.GetFinancialSummary;

public sealed record GetFinancialSummaryQuery(DateTime From, DateTime To)
    : IQuery<FinancialSummaryResponse>;
