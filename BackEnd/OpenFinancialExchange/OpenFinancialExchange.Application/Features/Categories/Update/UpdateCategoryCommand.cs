using OpenFinancialExchange.Application.Abstractions.Messaging;

namespace OpenFinancialExchange.Application.Features.Categories.Update;

public sealed record UpdateCategoryCommand(
    long Id,
    string Name,
    string Kind,
    string Color,
    bool IsInternal = false,
    string? Keywords = null) : ICommand;
