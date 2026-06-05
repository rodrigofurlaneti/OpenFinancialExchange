using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Domain.Events;

public sealed record TransactionCreatedEvent(
    Guid EventId,
    DateTime OccurredAt,
    string FITID,
    decimal Amount) : IDomainEvent;
