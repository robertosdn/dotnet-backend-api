using Backend.Domain.Common;

namespace Backend.Domain.AccessUsers;

public sealed class AccessUser
{
    public AccessUser(Guid id, AccessUserEmail email, AccessUserName name, string passwordHash, AccessUserStatus status, long version, DateTime createdAt, DateTime updatedAt)
    {
        Id = id;
        Email = email;
        Name = name;
        PasswordHash = passwordHash;
        Status = status;
        Version = version;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid Id { get; }
    public AccessUserEmail Email { get; }
    public AccessUserName Name { get; }
    public string PasswordHash { get; }
    public AccessUserStatus Status { get; }
    public long Version { get; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }

    public static AccessUser Create(Guid id, AccessUserEmail email, AccessUserName name, string passwordHash, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainValidationException("password hash is required");
        }

        return new AccessUser(id, email, name, passwordHash, AccessUserStatus.Active, 1, now, now);
    }

    public AccessUserCreatedEvent CreateCreatedEvent()
    {
        return new AccessUserCreatedEvent(
            Guid.NewGuid(),
            Id,
            Email.Value,
            Name.Value,
            Status,
            Version,
            CreatedAt,
            UpdatedAt);
    }

    public (AccessUser UpdatedUser, AccessUserUpdatedEvent? UpdatedEvent) Update(
        AccessUserName? name,
        AccessUserEmail? email,
        AccessUserStatus? status,
        string? passwordHash,
        long expectedVersion,
        DateTime now)
    {
        if (Version != expectedVersion)
        {
            throw new DomainValidationException("concurrency conflict");
        }

        var newName = name ?? Name;
        var newEmail = email ?? Email;
        var newStatus = status ?? Status;
        var newPasswordHash = passwordHash ?? PasswordHash;

        var hasChanges = newName.Value != Name.Value
            || newEmail.Value != Email.Value
            || newStatus != Status
            || (passwordHash != null && newPasswordHash != PasswordHash);

        if (!hasChanges)
        {
            return (this, null);
        }

        var updatedUser = new AccessUser(
            Id,
            newEmail,
            newName,
            newPasswordHash,
            newStatus,
            Version + 1,
            CreatedAt,
            now);

        var updatedEvent = new AccessUserUpdatedEvent(
            Guid.NewGuid(),
            Id,
            updatedUser.Email.Value,
            updatedUser.Name.Value,
            updatedUser.Status,
            updatedUser.Version,
            updatedUser.UpdatedAt,
            now);

        return (updatedUser, updatedEvent);
    }
}