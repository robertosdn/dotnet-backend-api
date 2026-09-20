namespace Backend.Contracts.AccessUsers;

public sealed record UpdateAccessUserRequest(
    long Version,
    string? Name,
    string? Email,
    string? Status,
    string? Password);