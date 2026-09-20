namespace Backend.Domain.AccessUsers;

public sealed record AccessUserCreatedEvent(
    Guid EventId,
    Guid AggregateId,
    string Email,
    string Name,
    AccessUserStatus Status,
    long Version,
    DateTime CreatedAt,
    DateTime OccurredAt);