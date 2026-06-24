using OpenFinancialExchange.Application.Abstractions.Messaging;
using OpenFinancialExchange.Domain.Primitives;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Application.Features.Dashboard.GetFinancialSummary;

internal sealed class GetFinancialSummaryQueryHandler(IOfxTransactionRepository repository)
    : IQueryHandler<GetFinancialSummaryQuery, FinancialSummaryResponse>
{
    public async Task<Result<FinancialSummaryResponse>> Handle(
        GetFinancialSummaryQuery request, CancellationToken cancellationToken)
    {
        var data = await repository.GetSummaryAsync(request.From, request.To, cancellationToken);

        var response = new FinancialSummaryResponse(
            TotalCredits:     data.TotalCredits,
            TotalDebits:      data.TotalDebits,
            NetBalance:       data.TotalCredits - data.TotalDebits,
            TransactionCount: data.TransactionCount,
            ByType:           data.ByType.Select(x => new TypeSummaryItem(x.TrnType, x.Total, x.Count)).ToList(),
            From:             request.From,
            To:               request.To);

        return Result.Success(response);
    }
}
