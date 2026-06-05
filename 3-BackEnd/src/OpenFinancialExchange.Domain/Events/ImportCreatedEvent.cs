using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Domain.Events;

public sealed record ImportCreatedEvent(
    Guid EventId,
    DateTime OccurredAt,
    string FileName) : IDomainEvent;
