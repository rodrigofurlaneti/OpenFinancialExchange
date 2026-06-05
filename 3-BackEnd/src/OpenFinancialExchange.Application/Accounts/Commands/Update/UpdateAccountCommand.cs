using MediatR;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Accounts.Commands.Update;

public sealed record UpdateAccountCommand(
    int Id,
    string? BranchNumber,
    string AccountNumber,
    string AccountType,
    string DefaultCurrency) : IRequest<Result>;
