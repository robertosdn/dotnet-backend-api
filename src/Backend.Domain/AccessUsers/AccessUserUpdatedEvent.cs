namespace Backend.Domain.AccessUsers;

public sealed record AccessUserUpdatedEvent(
    Guid EventId,
    Guid AggregateId,
    string Email,
    string Name,
    AccessUserStatus Status,
    long Version,
    DateTime UpdatedAt,
    DateTime OccurredAt);