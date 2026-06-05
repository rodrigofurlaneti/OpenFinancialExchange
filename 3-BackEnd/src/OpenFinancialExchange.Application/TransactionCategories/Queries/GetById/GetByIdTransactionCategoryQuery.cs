using MediatR;
using OpenFinancialExchange.Application.TransactionCategories.DTOs;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.TransactionCategories.Queries.GetById;

public sealed record GetByIdTransactionCategoryQuery(int Id)
    : IRequest<Result<TransactionCategoryDto>>;
