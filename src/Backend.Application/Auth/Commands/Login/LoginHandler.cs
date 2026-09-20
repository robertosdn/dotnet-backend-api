using Backend.Application.Abstractions.Persistence;
using Backend.Application.Abstractions.Security;
using Backend.Contracts.Auth;
using Backend.Domain.AccessUsers;
using Backend.Domain.Common;

namespace Backend.Application.Auth.Commands.Login;

public sealed class LoginHandler(
    IAccessUserWriteRepository repository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService)
{
    public async Task<LoginResult> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        var errors = Validate(command);
        if (errors.Count > 0)
        {
            return LoginResult.Validation(errors);
        }

        try
        {
            var email = AccessUserEmail.Parse(command.Email);
            var user = await repository.FindByEmailAsync(email.Value, cancellationToken);

            if (user is null)
            {
                return LoginResult.InvalidCredentials();
            }

            if (user.Status == AccessUserStatus.Disabled)
            {
                return LoginResult.UserDisabled();
            }

            var isValid = await passwordHasher.VerifyAsync(command.Password, user.PasswordHash, cancellationToken);
            if (!isValid)
            {
                return LoginResult.InvalidCredentials();
            }

            var expiration = TimeSpan.FromMinutes(60);
            var accessToken = tokenService.GenerateToken(user.Id, user.Email.Value, user.Name.Value, expiration);
            var response = new LoginResponse(accessToken, "Bearer", (int)expiration.TotalSeconds);

            return LoginResult.Success(response);
        }
        catch (DomainValidationException exception)
        {
            return LoginResult.Validation(new Dictionary<string, string[]>
            {
                ["request"] = [exception.Message]
            });
        }
    }

    private static Dictionary<string, string[]> Validate(LoginCommand command)
    {
        var errors = new Dictionary<string, string[]>();

        var email = command.Email?.Trim() ?? string.Empty;
        var at = email.IndexOf('@');
        var dot = email.LastIndexOf('.');
        if (email.Length > 254 || at <= 0 || dot <= at + 1 || dot >= email.Length - 1 || email.Any(char.IsWhiteSpace))
        {
            errors["email"] = ["Email is required and must be valid."];
        }

        if (string.IsNullOrWhiteSpace(command.Password))
        {
            errors["password"] = ["Password is required."];
        }

        return errors;
    }
}