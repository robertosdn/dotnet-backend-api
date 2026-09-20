namespace Backend.Contracts.Auth;

public sealed record LoginResponse(string AccessToken, string TokenType, int ExpiresIn);