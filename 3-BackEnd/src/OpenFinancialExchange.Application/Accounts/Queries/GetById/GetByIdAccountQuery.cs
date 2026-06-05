using MediatR;
using OpenFinancialExchange.Application.Accounts.DTOs;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Accounts.Queries.GetById;

public sealed record GetByIdAccountQuery(int Id) : IRequest<Result<AccountDto>>;
