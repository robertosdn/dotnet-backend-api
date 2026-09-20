namespace Backend.Contracts.AccessUsers;

public sealed record CreateAccessUserRequest(string? Name, string? Email, string? Password);