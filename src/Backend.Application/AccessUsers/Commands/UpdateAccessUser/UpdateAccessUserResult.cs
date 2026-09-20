using Backend.Contracts.AccessUsers;

namespace Backend.Application.AccessUsers.Commands.UpdateAccessUser;

public sealed class UpdateAccessUserResult
{
    public AccessUserResponse? Response { get; }
    public Dictionary<string, string[]>? Errors { get; }
    public string? ErrorCode { get; }

    private UpdateAccessUserResult(AccessUserResponse? response, Dictionary<string, string[]>? errors, string? errorCode)
    {
        Response = response;
        Errors = errors;
        ErrorCode = errorCode;
    }

    public static UpdateAccessUserResult Success(AccessUserResponse response) =>
        new(response, null, null);

    public static UpdateAccessUserResult Validation(Dictionary<string, string[]> errors) =>
        new(null, errors, "validation");

    public static UpdateAccessUserResult NotFound() =>
        new(null, null, "not_found");

    public static UpdateAccessUserResult ConcurrencyConflict() =>
        new(null, null, "concurrency_conflict");

    public static UpdateAccessUserResult DuplicateEmail() =>
        new(null, null, "duplicate_email");

    public static UpdateAccessUserResult StorageFailure() =>
        new(null, null, "storage_failure");
}