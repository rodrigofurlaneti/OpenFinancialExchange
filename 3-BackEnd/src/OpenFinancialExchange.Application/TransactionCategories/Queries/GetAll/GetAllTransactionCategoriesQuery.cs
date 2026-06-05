using MediatR;
using OpenFinancialExchange.Application.TransactionCategories.DTOs;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.TransactionCategories.Queries.GetAll;

public sealed record GetAllTransactionCategoriesQuery : IRequest<Result<IReadOnlyList<TransactionCategoryDto>>>;
