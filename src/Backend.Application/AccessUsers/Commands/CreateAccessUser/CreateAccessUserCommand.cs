namespace Backend.Application.AccessUsers.Commands.CreateAccessUser;

public sealed record CreateAccessUserCommand(string? Name, string? Email, string? Password);