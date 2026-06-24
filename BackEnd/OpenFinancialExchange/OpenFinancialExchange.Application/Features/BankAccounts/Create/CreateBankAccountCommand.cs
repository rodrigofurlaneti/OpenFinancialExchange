using OpenFinancialExchange.Application.Abstractions.Messaging;

namespace OpenFinancialExchange.Application.Features.BankAccounts.Create;

public sealed record CreateBankAccountCommand(
    long FinancialInstitutionId,
    string BankId,
    string? BranchId,
    string AcctId,
    string AcctType) : ICommand<long>;
