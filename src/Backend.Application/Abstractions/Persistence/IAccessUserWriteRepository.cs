using Backend.Domain.AccessUsers;

namespace Backend.Application.Abstractions.Persistence;

public enum SaveAccessUserError
{
    None,
    DuplicateEmail,
    ConcurrencyConflict,
    NotFound,
    Storage
}

public interface IAccessUserWriteRepository
{
    Task<SaveAccessUserError> SaveAsync(
        AccessUser user,
        AccessUserCreatedEvent createdEvent,
        CancellationToken cancellationToken);

    Task<SaveAccessUserError> UpdateAsync(
        AccessUser currentUser,
        AccessUser updatedUser,
        AccessUserUpdatedEvent? updatedEvent,
        long expectedVersion,
        CancellationToken cancellationToken);

    Task<AccessUser?> FindByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<AccessUser?> FindByEmailAsync(string email, CancellationToken cancellationToken);
}