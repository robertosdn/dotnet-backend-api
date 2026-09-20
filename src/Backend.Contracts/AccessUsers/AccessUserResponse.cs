namespace Backend.Contracts.AccessUsers;

public sealed record AccessUserResponse(
    Guid Id,
    string Email,
    string Name,
    string Status,
    long Version,
    DateTime CreatedAt,
    DateTime UpdatedAt);