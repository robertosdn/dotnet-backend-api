using Backend.Contracts.AccessUsers;

namespace Backend.Application.AccessUsers.Queries.ListAccessUsers;

public sealed record ListAccessUsersResult(
    bool Succeeded,
    ListAccessUsersResponse? Response,
    string? ErrorCode,
    IReadOnlyDictionary<string, string[]> Errors)
{
    public static ListAccessUsersResult Success(ListAccessUsersResponse response) =>
        new(true, response, null, new Dictionary<string, string[]>());

    public static ListAccessUsersResult Validation(IReadOnlyDictionary<string, string[]> errors) =>
        new(false, null, "validation", errors);

    public static ListAccessUsersResult StorageFailure() =>
        new(false, null, "storage", new Dictionary<string, string[]>());
}
