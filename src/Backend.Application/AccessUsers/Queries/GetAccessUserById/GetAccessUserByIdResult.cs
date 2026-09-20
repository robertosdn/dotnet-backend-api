using Backend.Contracts.AccessUsers;

namespace Backend.Application.AccessUsers.Queries.GetAccessUserById;

public sealed record GetAccessUserByIdResult(
    bool Succeeded,
    AccessUserResponse? Response,
    string? ErrorCode)
{
    public static GetAccessUserByIdResult Success(AccessUserResponse response) => new(true, response, null);

    public static GetAccessUserByIdResult NotFound() => new(false, null, "not_found");

    public static GetAccessUserByIdResult StorageFailure() => new(false, null, "storage");
}