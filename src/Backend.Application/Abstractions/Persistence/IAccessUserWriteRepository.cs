using Backend.Domain.AccessUsers;

namespace Backend.Application.Abstractions.Persistence;

public enum SaveAccessUserError
{
    None,
    DuplicateEmail,
    Storage
}

public interface IAccessUserWriteRepository
{
    Task<SaveAccessUserError> SaveAsync(
        AccessUser user,
        AccessUserCreatedEvent createdEvent,
        CancellationToken cancellationToken);
}