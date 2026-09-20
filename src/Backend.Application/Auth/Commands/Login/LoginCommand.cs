namespace Backend.Application.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password);