namespace Backend.Application.AccessUsers.Queries.ListAccessUsers;

public sealed record ListAccessUsersQuery(int Page, int PageSize, string? Status, string? Email);
