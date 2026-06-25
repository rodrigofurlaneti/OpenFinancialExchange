using OpenFinancialExchange.Application.Abstractions.Messaging;

namespace OpenFinancialExchange.Application.Features.Categories.Delete;

public sealed record DeleteCategoryCommand(long Id) : ICommand;
