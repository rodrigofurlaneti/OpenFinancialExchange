using OpenFinancialExchange.Application.Abstractions.Messaging;

namespace OpenFinancialExchange.Application.Features.BankAccounts.GetAll;

public sealed record GetAllBankAccountsQuery : IQuery<IReadOnlyCollection<BankAccountResponse>>;
