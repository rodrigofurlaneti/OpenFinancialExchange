using MediatR;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Accounts.Commands.Create;

public sealed record CreateAccountCommand(
    int ImportId,
    int BankId,
    string? BranchNumber,
    string AccountNumber,
    string AccountType,
    string DefaultCurrency) : IRequest<Result<int>>;
