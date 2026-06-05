using MediatR;
using OpenFinancialExchange.Application.Banks.DTOs;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Banks.Queries.GetById;

public sealed record GetByIdBankQuery(int Id) : IRequest<Result<BankDto>>;
