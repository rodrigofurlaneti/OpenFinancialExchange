using OpenFinancialExchange.Application.Abstractions.Messaging;

namespace OpenFinancialExchange.Application.Features.OfxTransactions.GetByBankAccount;

public sealed record GetOfxTransactionsByBankAccountQuery(
    long BankAccountId,
    DateTime? From,
    DateTime? To) : IQuery<IReadOnlyCollection<OfxTransactionResponse>>;
