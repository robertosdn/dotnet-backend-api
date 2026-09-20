using Backend.Contracts.Auth;

namespace Backend.Application.Auth.Commands.Login;

public sealed record LoginResult(
    bool Succeeded,
    LoginResponse? Response,
    string? ErrorCode,
    IReadOnlyDictionary<string, string[]> Errors)
{
    public static LoginResult Success(LoginResponse response) => new(true, response, null, new Dictionary<string, string[]>());

    public static LoginResult Validation(IReadOnlyDictionary<string, string[]> errors) => new(false, null, "validation", errors);

    public static LoginResult InvalidCredentials() => new(false, null, "invalid_credentials", new Dictionary<string, string[]>());

    public static LoginResult UserDisabled() => new(false, null, "user_disabled", new Dictionary<string, string[]>());

    public static LoginResult StorageFailure() => new(false, null, "storage", new Dictionary<string, string[]>());
}