using Backend.Application.Abstractions.Persistence;
using Backend.Contracts.AccessUsers;

namespace Backend.Application.AccessUsers.Queries.ListAccessUsers;

public sealed class ListAccessUsersHandler(IAccessUserReadRepository repository)
{
    public async Task<ListAccessUsersResult> HandleAsync(
        ListAccessUsersQuery query,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();

        if (query.Page < 1)
        {
            errors["page"] = ["Page must be greater than or equal to 1."];
        }

        if (query.PageSize < 1 || query.PageSize > 100)
        {
            errors["pageSize"] = ["Page size must be between 1 and 100."];
        }

        string? status = null;
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            status = query.Status.Trim().ToLowerInvariant();
            if (status is not ("active" or "disabled"))
            {
                errors["status"] = ["Status must be 'active' or 'disabled'."];
            }
        }

        string? email = null;
        if (!string.IsNullOrWhiteSpace(query.Email))
        {
            email = query.Email.Trim().ToLowerInvariant();
            var at = email.IndexOf('@');
            var dot = email.LastIndexOf('.');
            if (email.Length > 254 || at <= 0 || dot <= at + 1 || dot >= email.Length - 1 || email.Any(char.IsWhiteSpace))
            {
                errors["email"] = ["Email filter must be a valid email address."];
            }
        }

        if (errors.Count > 0)
        {
            return ListAccessUsersResult.Validation(errors);
        }

        try
        {
            var (items, totalCount) = await repository.SearchAsync(
                status,
                email,
                query.Page,
                query.PageSize,
                cancellationToken);

            return ListAccessUsersResult.Success(new ListAccessUsersResponse(
                items,
                query.Page,
                query.PageSize,
                totalCount));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return ListAccessUsersResult.StorageFailure();
        }
    }
}
