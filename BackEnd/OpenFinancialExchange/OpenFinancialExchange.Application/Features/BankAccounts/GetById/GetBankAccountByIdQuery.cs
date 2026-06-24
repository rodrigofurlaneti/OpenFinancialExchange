using OpenFinancialExchange.Application.Abstractions.Messaging;

namespace OpenFinancialExchange.Application.Features.BankAccounts.GetById;

public sealed record GetBankAccountByIdQuery(long Id) : IQuery<BankAccountResponse>;
