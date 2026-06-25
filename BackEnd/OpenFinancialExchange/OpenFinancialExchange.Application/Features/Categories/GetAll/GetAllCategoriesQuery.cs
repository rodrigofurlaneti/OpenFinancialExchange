using OpenFinancialExchange.Application.Abstractions.Messaging;

namespace OpenFinancialExchange.Application.Features.Categories.GetAll;

public sealed record GetAllCategoriesQuery : IQuery<IReadOnlyCollection<CategoryResponse>>;
