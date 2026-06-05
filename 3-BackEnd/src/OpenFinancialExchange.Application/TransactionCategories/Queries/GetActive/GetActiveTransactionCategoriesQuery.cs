using MediatR;
using OpenFinancialExchange.Application.TransactionCategories.DTOs;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.TransactionCategories.Queries.GetActive;

public sealed record GetActiveTransactionCategoriesQuery
    : IRequest<Result<IReadOnlyList<TransactionCategoryDto>>>;
