using OpenFinancialExchange.Application.Abstractions.Messaging;

namespace OpenFinancialExchange.Application.Features.Categories.Create;

public sealed record CreateCategoryCommand(
    string Name,
    string Kind,
    string Color,
    bool IsInternal = false,
    string? Keywords = null) : ICommand<long>;
