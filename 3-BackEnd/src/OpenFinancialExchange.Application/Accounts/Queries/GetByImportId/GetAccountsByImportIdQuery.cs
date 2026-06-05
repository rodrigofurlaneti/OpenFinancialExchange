using MediatR;
using OpenFinancialExchange.Application.Accounts.DTOs;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Accounts.Queries.GetByImportId;

public sealed record GetAccountsByImportIdQuery(int ImportId)
    : IRequest<Result<IReadOnlyList<AccountDto>>>;
