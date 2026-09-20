using Backend.Application.Abstractions.Persistence;
using Backend.Application.Abstractions.Security;
using Backend.Contracts.AccessUsers;
using Backend.Domain.AccessUsers;
using Backend.Domain.Common;

namespace Backend.Application.AccessUsers.Commands.CreateAccessUser;

public sealed class CreateAccessUserHandler(
    IAccessUserWriteRepository repository,
    IPasswordHasher passwordHasher)
{
    public async Task<CreateAccessUserResult> HandleAsync(
        CreateAccessUserCommand command,
        CancellationToken cancellationToken)
    {
        var errors = Validate(command);
        if (errors.Count > 0)
        {
            return CreateAccessUserResult.Validation(errors);
        }

        try
        {
            var email = AccessUserEmail.Parse(command.Email);
            var name = AccessUserName.Parse(command.Name);
            var passwordHash = await passwordHasher.HashAsync(command.Password!, cancellationToken);
            var now = DateTime.UtcNow;
            var user = AccessUser.Create(Guid.NewGuid(), email, name, passwordHash, now);
            var saveError = await repository.SaveAsync(user, user.CreateCreatedEvent(), cancellationToken);

            return saveError switch
            {
                SaveAccessUserError.None => CreateAccessUserResult.Success(ToResponse(user)),
                SaveAccessUserError.DuplicateEmail => CreateAccessUserResult.DuplicateEmail(),
                _ => CreateAccessUserResult.StorageFailure()
            };
        }
        catch (DomainValidationException exception)
        {
            return CreateAccessUserResult.Validation(new Dictionary<string, string[]>
            {
                ["request"] = [exception.Message]
            });
        }
    }

    private static Dictionary<string, string[]> Validate(CreateAccessUserCommand command)
    {
        var errors = new Dictionary<string, string[]>();
        var password = command.Password ?? string.Empty;
        var hasUpper = password.Any(char.IsUpper);
        var hasLower = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSymbol = password.Any(character => !char.IsLetterOrDigit(character));

        if (string.IsNullOrWhiteSpace(command.Name) || command.Name.Trim().Length > 255)
        {
            errors["name"] = ["Name is required and must contain at most 255 characters."];
        }

        var email = command.Email?.Trim() ?? string.Empty;
        var at = email.IndexOf('@');
        var dot = email.LastIndexOf('.');
        if (email.Length > 254 || at <= 0 || dot <= at + 1 || dot >= email.Length - 1 || email.Any(char.IsWhiteSpace))
        {
            errors["email"] = ["Email is required and must be valid."];
        }

        if (password.Length < 8 || !hasUpper || !hasLower || !hasDigit || !hasSymbol)
        {
            errors["password"] = ["Password must contain at least 8 characters, including uppercase, lowercase, digit and symbol."];
        }

        return errors;
    }

    private static AccessUserResponse ToResponse(AccessUser user) => new(
        user.Id,
        user.Email.Value,
        user.Name.Value,
        user.Status.ToString().ToLowerInvariant(),
        user.Version,
        user.CreatedAt,
        user.UpdatedAt);
}