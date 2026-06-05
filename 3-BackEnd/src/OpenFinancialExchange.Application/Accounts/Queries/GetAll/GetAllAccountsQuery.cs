using MediatR;
using OpenFinancialExchange.Application.Accounts.DTOs;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Accounts.Queries.GetAll;

public sealed record GetAllAccountsQuery : IRequest<Result<IReadOnlyList<AccountDto>>>;
