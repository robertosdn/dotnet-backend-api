namespace Backend.Application.AccessUsers.Commands.UpdateAccessUser;

public sealed record UpdateAccessUserCommand(
    Guid Id,
    long Version,
    string? Name,
    string? Email,
    string? Status,
    string? Password);