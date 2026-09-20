namespace Backend.Contracts.AccessUsers;

public sealed record ListAccessUsersResponse(
    IReadOnlyList<AccessUserResponse> Items,
    int Page,
    int PageSize,
    long TotalCount);
