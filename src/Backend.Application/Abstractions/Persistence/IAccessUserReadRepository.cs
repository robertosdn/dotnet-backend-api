using Backend.Contracts.AccessUsers;

namespace Backend.Application.Abstractions.Persistence;

public interface IAccessUserReadRepository
{
    Task<AccessUserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}