using OpenFinancialExchange.Application.Abstractions.Messaging;

namespace OpenFinancialExchange.Application.Features.OfxTransactions.GetByStatement;

public sealed record GetOfxTransactionsByStatementQuery(long StatementId)
    : IQuery<IReadOnlyCollection<OfxTransactionResponse>>;
