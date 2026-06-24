using OpenFinancialExchange.Application.Abstractions.Messaging;

namespace OpenFinancialExchange.Application.Features.BankAccounts.Update;

public sealed record UpdateBankAccountCommand(long Id, string? BranchId, string AcctType) : ICommand;
