using Backend.Contracts.AccessUsers;

namespace Backend.Application.Abstractions.Persistence;

public interface IAccessUserReadRepository
{
    Task<AccessUserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<(IReadOnlyList<AccessUserResponse> Items, long TotalCount)> SearchAsync(
        string? status,
        string? email,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}