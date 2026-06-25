using OpenFinancialExchange.Application.Abstractions.Messaging;

namespace OpenFinancialExchange.Application.Features.OfxTransactions.AssignCategory;

/// <summary>Atribui (ou remove, com <see cref="CategoryId"/> = null) a categoria de uma transação.</summary>
public sealed record AssignCategoryCommand(
    long TransactionId,
    long? CategoryId) : ICommand;
