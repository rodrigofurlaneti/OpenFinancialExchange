using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Domain.Events;

public sealed record TransactionCategoryCreatedEvent(
    Guid EventId,
    DateTime OccurredAt,
    string Code) : IDomainEvent;
