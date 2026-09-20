using Backend.Application.Abstractions.Persistence;
using Backend.Application.Abstractions.Security;
using Backend.Contracts.AccessUsers;
using Backend.Domain.AccessUsers;
using Backend.Domain.Common;

namespace Backend.Application.AccessUsers.Commands.UpdateAccessUser;

public sealed class UpdateAccessUserHandler(
    IAccessUserWriteRepository repository,
    IPasswordHasher passwordHasher)
{
    public async Task<UpdateAccessUserResult> HandleAsync(
        UpdateAccessUserCommand command,
        CancellationToken cancellationToken)
    {
        var errors = Validate(command);
        if (errors.Count > 0)
        {
            return UpdateAccessUserResult.Validation(errors);
        }

        try
        {
            var user = await repository.FindByIdAsync(command.Id, cancellationToken);
            if (user == null)
            {
                return UpdateAccessUserResult.NotFound();
            }

            var now = DateTime.UtcNow;
            AccessUserName? name = null;
            AccessUserEmail? email = null;
            AccessUserStatus? status = null;
            string? passwordHash = null;

            if (command.Name != null)
            {
                name = AccessUserName.Parse(command.Name);
            }

            if (command.Email != null)
            {
                email = AccessUserEmail.Parse(command.Email);
            }

            if (command.Status != null)
            {
                if (!Enum.TryParse<AccessUserStatus>(command.Status, true, out var parsedStatus))
                {
                    return UpdateAccessUserResult.Validation(new Dictionary<string, string[]>
                    {
                        ["status"] = ["Status must be 'active' or 'disabled'."]
                    });
                }
                status = parsedStatus;
            }

            if (command.Password != null)
            {
                passwordHash = await passwordHasher.HashAsync(command.Password, cancellationToken);
            }

            var (updatedUser, updatedEvent) = user.Update(
                name,
                email,
                status,
                passwordHash,
                command.Version,
                now);

            if (updatedEvent == null)
            {
                return UpdateAccessUserResult.Success(ToResponse(updatedUser));
            }

            var saveError = await repository.UpdateAsync(
                user,
                updatedUser,
                updatedEvent,
                command.Version,
                cancellationToken);

            return saveError switch
            {
                SaveAccessUserError.None => UpdateAccessUserResult.Success(ToResponse(updatedUser)),
                SaveAccessUserError.ConcurrencyConflict => UpdateAccessUserResult.ConcurrencyConflict(),
                SaveAccessUserError.DuplicateEmail => UpdateAccessUserResult.DuplicateEmail(),
                _ => UpdateAccessUserResult.StorageFailure()
            };
        }
        catch (DomainValidationException exception)
        {
            return UpdateAccessUserResult.Validation(new Dictionary<string, string[]>
            {
                ["request"] = [exception.Message]
            });
        }
    }

    private static Dictionary<string, string[]> Validate(UpdateAccessUserCommand command)
    {
        var errors = new Dictionary<string, string[]>();

        if (command.Name != null && (string.IsNullOrWhiteSpace(command.Name) || command.Name.Trim().Length > 255))
        {
            errors["name"] = ["Name must contain at most 255 characters."];
        }

        if (command.Email != null)
        {
            var email = command.Email.Trim();
            var at = email.IndexOf('@');
            var dot = email.LastIndexOf('.');
            if (email.Length > 254 || at <= 0 || dot <= at + 1 || dot >= email.Length - 1 || email.Any(char.IsWhiteSpace))
            {
                errors["email"] = ["Email must be valid."];
            }
        }

        if (command.Password != null)
        {
            var password = command.Password;
            var hasUpper = password.Any(char.IsUpper);
            var hasLower = password.Any(char.IsLower);
            var hasDigit = password.Any(char.IsDigit);
            var hasSymbol = password.Any(character => !char.IsLetterOrDigit(character));

            if (password.Length < 8 || !hasUpper || !hasLower || !hasDigit || !hasSymbol)
            {
                errors["password"] = ["Password must contain at least 8 characters, including uppercase, lowercase, digit and symbol."];
            }
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