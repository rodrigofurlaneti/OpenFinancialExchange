using MediatR;
using OpenFinancialExchange.Application.Banks.DTOs;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Banks.Queries.GetAll;

public sealed record GetAllBanksQuery : IRequest<Result<IReadOnlyList<BankDto>>>;
