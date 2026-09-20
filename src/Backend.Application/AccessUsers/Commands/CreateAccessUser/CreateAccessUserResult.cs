using Backend.Contracts.AccessUsers;

namespace Backend.Application.AccessUsers.Commands.CreateAccessUser;

public sealed record CreateAccessUserResult(
    bool Succeeded,
    AccessUserResponse? Response,
    string? ErrorCode,
    IReadOnlyDictionary<string, string[]> Errors)
{
    public static CreateAccessUserResult Success(AccessUserResponse response) => new(true, response, null, new Dictionary<string, string[]>());

    public static CreateAccessUserResult Validation(IReadOnlyDictionary<string, string[]> errors) => new(false, null, "validation", errors);

    public static CreateAccessUserResult DuplicateEmail() => new(false, null, "duplicate_email", new Dictionary<string, string[]>());

    public static CreateAccessUserResult StorageFailure() => new(false, null, "storage", new Dictionary<string, string[]>());
}