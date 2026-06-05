using MediatR;
using OpenFinancialExchange.Application.Accounts.DTOs;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Accounts.Queries.GetByBankId;

public sealed record GetAccountsByBankIdQuery(int BankId)
    : IRequest<Result<IReadOnlyList<AccountDto>>>;
