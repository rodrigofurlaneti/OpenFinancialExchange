using OpenFinancialExchange.Application.Abstractions.Messaging;
using OpenFinancialExchange.Domain.Primitives;
using OpenFinancialExchange.Domain.Repositories;

namespace OpenFinancialExchange.Application.Features.OfxTransactions.GetByBankAccount;

internal sealed class GetOfxTransactionsByBankAccountQueryHandler(
    IOfxTransactionRepository repository,
    ICategoryRepository categoryRepository)
    : IQueryHandler<GetOfxTransactionsByBankAccountQuery, IReadOnlyCollection<OfxTransactionResponse>>
{
    public async Task<Result<IReadOnlyCollection<OfxTransactionResponse>>> Handle(
        GetOfxTransactionsByBankAccountQuery request, CancellationToken cancellationToken)
    {
        var transactions = await repository.GetByBankAccountAsync(
            request.BankAccountId, request.From, request.To, cancellationToken);

        var categories = await categoryRepository.GetAllAsync(cancellationToken);
        var categoryNames = categories.ToDictionary(c => c.Id, c => c.Name);

        var response = transactions
            .OrderByDescending(t => t.DtPosted)
            .Select(t => new OfxTransactionResponse(t.Id, t.StatementId, t.TrnType, t.DtPosted,
                t.TrnAmt, t.FitId, t.Name, t.Memo, t.CheckNum, t.CategoryId,
                t.CategoryId is { } cid && categoryNames.TryGetValue(cid, out var n) ? n : null,
                t.CreatedAt))
            .ToList().AsReadOnly();

        return Result.Success<IReadOnlyCollection<OfxTransactionResponse>>(response);
    }
}
